using course_project.Models;
using course_project.Services;

namespace course_project.Forms;

public partial class SaleForm : Form
{
    private readonly SaleService _saleService;
    private readonly ProductService _productService;
    private List<SaleItemDisplay> _currentSaleItems; // Товары в текущем чеке
    private List<Product> _foundProducts; // Для отображения результатов поиска
    private User _currentUser => SessionManager.CurrentUser;
    private bool _isReady = false;
    private const string SearchPlaceholder = "Введите название";
    
    public SaleForm()
    {
        InitializeComponent();
        
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        
        _saleService = new SaleService();
        _productService = new ProductService();
        
        // Привязываем события
        buttonAddProduct.Click += buttonAddProduct_Click;
        lstFoundProducts.DoubleClick += (s, e) => buttonAddProduct.PerformClick();
        
        // ВАЖНО: Подписываемся на клик по таблице здесь, а не в настройке колонок
        // Это предотвращает удаление двух товаров за раз
        dgvSaleItems.CellClick += dgvSaleItems_CellClick;
    }

    private void SellerSaleForm_Load(object sender, EventArgs e)
    {
        InitializeSaleForm();
        _isReady = true;
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

        // Обновляем текст в нашей красивой шапке
        labelHeader.Text = $"Оформление продажи | Кассир: {_currentUser.UserName}";

        _currentSaleItems = new List<SaleItemDisplay>();
        _foundProducts = new List<Product>();

        dgvSaleItems.AutoGenerateColumns = false;
        
        SetupDataGridViewColumns();
        UpdateSaleDisplay();
        
        txtSearchProduct.Text = SearchPlaceholder;
        txtSearchProduct.ForeColor = SystemColors.ControlDark;
        txtSearchProduct.GotFocus += TxtSearchProduct_GotFocus;
        txtSearchProduct.LostFocus += TxtSearchProduct_LostFocus;

        lstFoundProducts.DisplayMember = "DisplayInfo";
        lstFoundProducts.ValueMember = "ProductId";

        LoadCategoriesIntoComboBox();
    }

    private void comboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
    {
        if (_isReady)
            PerformProductSearch();
    }
    
    private void LoadCategoriesIntoComboBox()
    {
        cmbCategory.Items.Clear();
        cmbCategory.Items.Add("Все категории");
        List<string> categories = _productService.GetAllCategories();
        foreach (string category in categories)
        {
            cmbCategory.Items.Add(category);
        }
        cmbCategory.SelectedIndex = 0;
    }
    
    private void SetupDataGridViewColumns()
    {
        dgvSaleItems.Columns.Clear();

        dgvSaleItems.Columns.Add("ProductId", "ID");
        dgvSaleItems.Columns.Add("ProductName", "Товар");
        dgvSaleItems.Columns.Add("Quantity", "Количество");
        dgvSaleItems.Columns.Add("Price", "Цена");
        dgvSaleItems.Columns.Add("Total", "Сумма");
        
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
    }
    
    private void UpdateSaleDisplay()
    {
        dgvSaleItems.Rows.Clear();

        foreach (var item in _currentSaleItems)
        {
            dgvSaleItems.Rows.Add(
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

        string rawQuery = txtSearchProduct.Text.Trim();
        bool isPlaceholderActive = txtSearchProduct.ForeColor == SystemColors.ControlDark && rawQuery == SearchPlaceholder;
        string query = isPlaceholderActive ? null : (string.IsNullOrWhiteSpace(rawQuery) ? null : rawQuery);
        
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
        if (txtSearchProduct.Text == SearchPlaceholder)
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
    
    // Обработчик удаления (теперь срабатывает 1 раз)
    private void dgvSaleItems_CellClick(object sender, DataGridViewCellEventArgs e)
    {
        // Проверка: клик не по заголовку
        if (e.RowIndex < 0 || e.ColumnIndex < 0) return;
        
        // Проверка: нажата колонка с кнопкой удаления
        if (dgvSaleItems.Columns[e.ColumnIndex].Name == "DeleteButton")
        {
            // Безопасное получение значения ID
            if (dgvSaleItems.Rows[e.RowIndex].Cells["ProductId"].Value != null &&
                int.TryParse(dgvSaleItems.Rows[e.RowIndex].Cells["ProductId"].Value.ToString(), out int productId))
            {
                var itemToRemove = _currentSaleItems.FirstOrDefault(i => i.ProductId == productId);
                if (itemToRemove != null)
                {
                    _currentSaleItems.Remove(itemToRemove);
                    UpdateSaleDisplay();
                }
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
        
        if (lstFoundProducts.SelectedValue == null) return;

        int selectedProductId = (int)lstFoundProducts.SelectedValue;
        var product = _foundProducts.FirstOrDefault(p => p.ProductId == selectedProductId);
        
        if (product == null)
        {
            MessageBox.Show("Не удалось найти выбранный товар.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        int quantity = (int)NMDQuantity.Value;

        // Проверяем, достаточно ли товара на складе (с учетом того, что уже в корзине)
        var existingItem = _currentSaleItems.FirstOrDefault(i => i.ProductId == product.ProductId);
        int quantityAlreadyInCart = existingItem?.Quantity ?? 0;

        if (quantity + quantityAlreadyInCart > product.QuantityInStock)
        {
            MessageBox.Show($"Недостаточно товара на складе. В наличии: {product.QuantityInStock}. В корзине: {quantityAlreadyInCart}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        
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
        this.Close();
        if (_currentUser.UserName == "admin")
        {
            ManagerMenuForm menu = new ManagerMenuForm();
            menu.Show();
        }
        else
        {
            SellerMenuForm form = new SellerMenuForm();
            form.Show();
        }
    }
}