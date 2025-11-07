using course_project.Models;
using course_project.Services;

namespace course_project.Forms;

public partial class SellerSaleForm : Form
{
    private readonly SaleService _saleService;
    private readonly ProductService _productService;
    private List<SaleItemDisplay> _currentSaleItems;//товары в текущем чеке
    private List<Product> _foundProducts;//для отображения результатов поиска товаров
    private User _currentUser => SessionManager.CurrentUser;//информация о текущем пользователе берется из SessionManager
    private bool _isInitializing = true;
    private bool _isReady = false;
    private bool _isAfterPayment = false;
    private const string SearchPlaceholder = "Введите название";
    
    public SellerSaleForm()
    {
        InitializeComponent();
        
        _saleService = new SaleService();
        _productService = new ProductService();

        InitializeSaleForm();
        buttonAddProduct.Click += buttonAddProduct_Click;
        lstFoundProducts.DoubleClick += (s, e) => buttonAddProduct.PerformClick();
        buttonPay.Click += buttonPay_Click;
    }

    private void SellerSaleForm_Load(object sender, EventArgs e)
    {
        InitializeSaleForm();
        _isReady = true;
        // показываем все товары по умолчанию после полной инициализации
        PerformProductSearch();
    }

    private void InitializeSaleForm()
    {
        if (_currentUser == null)
        {
            MessageBox.Show("Не удалось определить текущего пользователя. Пожалуйста, войдите в систему.",
                "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close();
            return;
        }

        _currentSaleItems = new List<SaleItemDisplay>();
        _foundProducts = new List<Product>();

        dgvSaleItems.AutoGenerateColumns = false;
        SetupDataGridViewColumns();
        UpdateSaleDisplay();

        // Настроим плейсхолдер для txtSearchProduct
        txtSearchProduct.Text = SearchPlaceholder;
        txtSearchProduct.ForeColor = SystemColors.ControlDark;
        txtSearchProduct.GotFocus += TxtSearchProduct_GotFocus;
        txtSearchProduct.LostFocus += TxtSearchProduct_LostFocus;

        lstFoundProducts.DisplayMember = "DisplayInfo";
        lstFoundProducts.ValueMember = "ProductId";

        LoadCategoriesIntoComboBox();

        this.Text = $"Оформление Продажи - Кассир: {_currentUser.UserName}";
    }

    private void comboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_isReady)
            PerformProductSearch();
    }
    
    private void LoadCategoriesIntoComboBox()
    {
        cmbCategory.Items.Clear();
        cmbCategory.Items.Add("Все категории"); // Добавляем опцию для показа всех категорий
        List<string> categories = _productService.GetAllCategories();
        foreach (string category in categories)
        {
            cmbCategory.Items.Add(category);
        }
        cmbCategory.SelectedIndex = 0; // Выбираем "Все категории" по умолчанию
    }
    
    private void SetupDataGridViewColumns()
    {
        dgvSaleItems.Columns.Clear();

        dgvSaleItems.Columns.Add("ProductId", "ID");
        dgvSaleItems.Columns.Add("ProductName", "Товар");
        dgvSaleItems.Columns.Add("Quantity", "Количество");
        dgvSaleItems.Columns.Add("Price", "Цена");
        dgvSaleItems.Columns.Add("Total", "Сумма");

        // Кнопка "Удалить"
        var deleteButton = new DataGridViewButtonColumn
        {
            Name = "DeleteButton",
            HeaderText = "",
            Text = "❌",
            UseColumnTextForButtonValue = true,
            Width = 70
        };
        dgvSaleItems.Columns.Add(deleteButton);

        dgvSaleItems.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        dgvSaleItems.CellClick += dgvSaleItems_CellClick;
    }
    
    private void UpdateSaleDisplay()
    {
        dgvSaleItems.Rows.Clear();

        foreach (var item in _currentSaleItems)
        {
            int rowIndex = dgvSaleItems.Rows.Add(
                item.ProductId,
                item.ProductName,
                item.Quantity,
                item.PriceAtTimeOfSale,
                item.PriceAtTimeOfSale * item.Quantity
            );
        }

        lblTotalAmount.Text = $"{_currentSaleItems.Sum(i => i.PriceAtTimeOfSale * i.Quantity):C2}";
    }
    
    private void PerformProductSearch()
    {
        if (!_isReady) return;

        string selectedCategory = cmbCategory.SelectedItem?.ToString();

        // считаем, что плейсхолдер — это пустой запрос
        string rawQuery = txtSearchProduct.Text.Trim();
        bool isPlaceholderActive = txtSearchProduct.ForeColor == SystemColors.ControlDark && rawQuery == SearchPlaceholder;
        string query = isPlaceholderActive ? null : (string.IsNullOrWhiteSpace(rawQuery) ? null : rawQuery);

        // если поле поиска пустое — передаём null, чтобы искать только по категории
        _foundProducts = _productService.SearchProducts(query, selectedCategory);

        if (_foundProducts.Any())
        {
            lstFoundProducts.DataSource = _foundProducts
                .Select(p => new
                {
                    p.ProductId,
                    DisplayInfo = $"{p.ProductName} ({p.ProductArticle}) - {p.Price:C2} (Наличие: {p.QuantityInStock})"
                })
                .ToList();
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(query) ||
                            (selectedCategory != null && selectedCategory != "Все категории"))
            {
                MessageBox.Show(
                    "Товар не найден в выбранной категории или по данному запросу, либо отсутствует на складе.",
                    "Информация",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            lstFoundProducts.DataSource = null;
            _foundProducts.Clear();
        }
    }

    private void buttonSearchProducts_Click(object sender, EventArgs e)
    {
        PerformProductSearch();
    }
    
    private void TxtSearchProduct_GotFocus(object sender, EventArgs e)
    {
        if (txtSearchProduct.Text == SearchPlaceholder && txtSearchProduct.ForeColor == SystemColors.ControlDark)
        {
            txtSearchProduct.Text = "";
            txtSearchProduct.ForeColor = SystemColors.WindowText;
        }
    }

    private void TxtSearchProduct_LostFocus(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSearchProduct.Text))
        {
            txtSearchProduct.Text = SearchPlaceholder;
            txtSearchProduct.ForeColor = SystemColors.ControlDark;
        }
    }
    
    private void dgvSaleItems_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        // Если клик по заголовку или вне таблицы — игнорируем
        if (e.RowIndex < 0 || e.ColumnIndex < 0)
            return;

        // Проверяем, что нажали на кнопку "Удалить"
        if (dgvSaleItems.Columns[e.ColumnIndex].Name == "DeleteButton")
        {
            int productId = (int)dgvSaleItems.Rows[e.RowIndex].Cells["ProductId"].Value;
            var itemToRemove = _currentSaleItems.FirstOrDefault(i => i.ProductId == productId);
            if (itemToRemove != null)
            {
                _currentSaleItems.Remove(itemToRemove);
                UpdateSaleDisplay();
            }
        }
    }
    
    private void buttonAddProduct_Click(object sender, EventArgs e)
    {
        if (lstFoundProducts.SelectedItem == null)
        {
            MessageBox.Show("Пожалуйста, выберите товар из списка.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Получаем выбранный товар
        int selectedProductId = (int)lstFoundProducts.SelectedValue;
        var product = _foundProducts.FirstOrDefault(p => p.ProductId == selectedProductId);
        if (product == null)
        {
            MessageBox.Show("Не удалось найти выбранный товар.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        int quantity = (int)NMDQuantity.Value;

        if (quantity > product.QuantityInStock)
        {
            MessageBox.Show("Недостаточно товара на складе.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // Проверяем, есть ли уже этот товар в текущем чеке
        var existingItem = _currentSaleItems.FirstOrDefault(i => i.ProductId == product.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            _currentSaleItems.Add(new SaleItemDisplay
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                Quantity = quantity,
                PriceAtTimeOfSale = product.Price
            });
        }

        UpdateSaleDisplay();
    }

    private void buttonPay_Click(object sender, EventArgs e)
    {
        if (_currentSaleItems.Count == 0)
        {
            MessageBox.Show("Добавьте хотя бы один товар перед оплатой.", "Ошибка", MessageBoxButtons.OK,
                MessageBoxIcon.Warning);
            return;
        }
        
        decimal totalPrice = _currentSaleItems.Sum(i => i.PriceAtTimeOfSale * i.Quantity);

        Sale sale = new Sale{
            SaleId = _saleService.GetNextSaleId(),
            SaleDate = DateTime.Now,
            SellerUserName = _currentUser.UserName,
            TotalAmount = totalPrice,
            Items = _currentSaleItems.Select(i=> new SaleItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity,
                PriceAtTimeOfSale = i.PriceAtTimeOfSale,
            }).ToList()
        };

        try
        {
            _saleService.AddSale(sale);

            foreach (var item in _currentSaleItems)
            {
                _productService.ReduceQuantity(item.ProductId, item.Quantity);
            }

            MessageBox.Show($"Продажа успешно сохранена!\nИтог: {totalPrice:C2}",
                "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);

            _currentSaleItems.Clear();
            UpdateSaleDisplay();
            PerformProductSearch();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при сохранении продажи: {ex.Message}",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void buttonMenu_Click(object sender, EventArgs e)
    {
        this.Hide();
        SellerMenuForm form = new SellerMenuForm();
        form.Show();
    }
}