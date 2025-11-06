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

    
    public SellerSaleForm()
    {
        InitializeComponent();
        
        _saleService = new SaleService();
        _productService = new ProductService();

        InitializeSaleForm();
    }

    private void InitializeSaleForm()
    {
        if (_currentUser == null)
        {
            MessageBox.Show("Не удалось определить текущего пользователя. Пожалуйста, войдите в систему.", "Ошибка авторизации", MessageBoxButtons.OK, MessageBoxIcon.Error);
            this.Close(); // Закрываем форму, если нет пользователя
            return;
        }

        _currentSaleItems = new List<SaleItemDisplay>();
        _foundProducts = new List<Product>();

        dgvSaleItems.AutoGenerateColumns = false;
        SetupDataGridViewColumns();
        UpdateSaleDisplay();

        // Настройка ListBox для отображения найденных товаров
        lstFoundProducts.DisplayMember = "DisplayInfo"; // Свойство DisplayInfo должно быть в анонимном типе
        lstFoundProducts.ValueMember = "ProductId";    // Id товара как значение
            
        // Загрузка категорий в ComboBox
        LoadCategoriesIntoComboBox();

        // Обновляем заголовок формы
        this.Text = $"Оформление Продажи - Кассир: {_currentUser.UserName}";

        // Выполняем начальный поиск для отображения всех товаров по умолчанию
        PerformProductSearch();
    }

    private void comboBoxCategory_SelectedIndexChanged(object sender, EventArgs e)
    {
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
        dgvSaleItems.Columns.Add(new DataGridViewTextBoxColumn { HeaderText = "№", ReadOnly = true, Width = 30 });
        dgvSaleItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Название", ReadOnly = true, Width = 150 });
        dgvSaleItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductArticle", HeaderText = "Артикул", ReadOnly = true, Width = 80 });
        dgvSaleItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Кол-во", Width = 60 });
        dgvSaleItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PriceAtTimeOfSale", HeaderText = "Цена за ед.", ReadOnly = true, Width = 80, DefaultCellStyle = { Format = "C2" } });
        dgvSaleItems.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalItemAmount", HeaderText = "Сумма", ReadOnly = true, Width = 100, DefaultCellStyle = { Format = "C2" } });

        DataGridViewButtonColumn deleteButton = new DataGridViewButtonColumn
        {
            Text = "Удалить",
            UseColumnTextForButtonValue = true,
            Name = "DeleteColumn",
            HeaderText = "",
            Width = 70
        };
        dgvSaleItems.Columns.Add(deleteButton);
    }
    
    private void UpdateSaleDisplay()
    {
        dgvSaleItems.Rows.Clear();
        decimal totalAmount = 0;

        for (int i = 0; i < _currentSaleItems.Count; i++)
        {
            var item = _currentSaleItems[i];
            dgvSaleItems.Rows.Add(
                i + 1,
                item.ProductName,
                item.ProductArticle,
                item.Quantity,
                item.PriceAtTimeOfSale,
                item.TotalItemAmount
            );
            totalAmount += item.TotalItemAmount;
        }
        lblTotalAmount.Text = totalAmount.ToString("C2");
        NMDQuantity.Value = 1; // Сбрасываем количество на 1 после добавления
    }
    
    private void PerformProductSearch()
    {
        string selectedCategory = cmbCategory.SelectedItem?.ToString();
        string query = txtSearchProduct.Text.Trim();

        _foundProducts = _productService.SearchProducts(query, selectedCategory);

        if (_foundProducts.Any())
        {
            // Создаем анонимный тип с DisplayInfo для ListBox
            lstFoundProducts.DataSource = _foundProducts.Select(p => new { p.ProductId, DisplayInfo = $"{p.ProductName} ({p.ProductArticle}) - {p.Price:C2} (Наличие: {p.QuantityInStock})" }).ToList();
            lstFoundProducts.DisplayMember = "DisplayInfo";
            lstFoundProducts.ValueMember = "ProductId";
        }
        else
        {
            MessageBox.Show("Товар не найден в выбранной категории или по данному запросу, либо отсутствует на складе.", "Информация", MessageBoxButtons.OK, MessageBoxIcon.Information);
            lstFoundProducts.DataSource = null;
            _foundProducts.Clear();
        }
    }

    private void buttonSearchProducts_Click(object sender, EventArgs e)
    {
        PerformProductSearch();
    }
    
}