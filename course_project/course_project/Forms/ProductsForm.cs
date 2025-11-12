using course_project.Models;
using course_project.Services;
using System.ComponentModel;
using System.Data;
using Microsoft.VisualBasic;

namespace course_project.Forms;

public partial class ProductsForm : Form
{
    private readonly ProductService _productService;
    private readonly UserService _userService;
    private bool _isTemporaryAdmin = false;

    public ProductsForm()
    {
        InitializeComponent();
        this.Load += new System.EventHandler(this.ProductsForm_Load);
        
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        
        _productService = new ProductService();
        _userService = new UserService();
    }

    private void ProductsForm_Load(object sender, EventArgs e)
    {
        NUDPriceFrom.Maximum = decimal.MaxValue;
        NUDPriceTo.Maximum = decimal.MaxValue;

        dataGridView1.AutoGenerateColumns = false;
        SetupDataGridViewColumns();
        
        LoadAllProducts();
        LoadCategories();
        
        ConfigureAccess();
        
        buttonSearchFilter.Click += (s, ev) => ApplyFilters();
        buttonSearch.Click += ButtonSearch_Click;
    }
    
    private void buttonAccess_Click(object sender, EventArgs e)
    {
        string password = Interaction.InputBox("Для получения прав администратора введите пароль:", "Подтверждение доступа", "");
        if (string.IsNullOrEmpty(password)) return;

        var adminUser = _userService.GetUsers().FirstOrDefault(u => u.UserName == "admin");
        
        if (adminUser == null)
        {
            MessageBox.Show("Пользователь с правами администратора не найден в системе.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        if (AuthService.VerifyPassword(password, adminUser.HashedPassword))
        {
            _isTemporaryAdmin = true;
            ConfigureAccess();
            MessageBox.Show("Доступ администратора предоставлен.", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show("Неверный пароль администратора.", "Ошибка доступа", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
    
    private void ConfigureAccess()
    {
        if (!SessionManager.IsLoggedIn)
        {
            panel1.Enabled = false;
            panel2.Enabled = false;
            buttonAccess.Visible = false;
            return;
        }
        
        bool hasAdminRights = (SessionManager.CurrentUser.UserName == "admin") || _isTemporaryAdmin;

        if (hasAdminRights)
        {
            dataGridView1.ReadOnly = false;
            dataGridView1.Columns["Increase"].Visible = true;
            dataGridView1.Columns["Decrease"].Visible = true;
            dataGridView1.Columns["DeleteButton"].Visible = true;
            buttonAccess.Visible = false;
        }
        else 
        {
            dataGridView1.ReadOnly = true;
            dataGridView1.Columns["Increase"].Visible = false;
            dataGridView1.Columns["Decrease"].Visible = false;
            dataGridView1.Columns["DeleteButton"].Visible = false;
            buttonAccess.Visible = true;
        }
        
        panel2.Enabled = true;
    }
    
    private void ButtonSearch_Click(object sender, EventArgs e)
    {
        int productId = (int)nudID.Value;
        var product = _productService.GetProductById(productId);
        
        var productList = new BindingList<Product>();
        if (product != null)
        {
            productList.Add(product);
        }
        else
        {
            MessageBox.Show($"Товар с ID = {productId} не найден.", "Поиск по ID", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        dataGridView1.DataSource = productList;
    }

    private void SetupDataGridViewColumns()
    {
        dataGridView1.Columns.Clear();

        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductId", DataPropertyName = "ProductId", HeaderText = "ID", ReadOnly = true, Width = 50 });
        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductArticle", DataPropertyName = "ProductArticle", HeaderText = "Артикул" });
        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "ProductName", DataPropertyName = "ProductName", HeaderText = "Название товара", Width = 250 });
        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "Category", DataPropertyName = "Category", HeaderText = "Категория" });
        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "Price", DataPropertyName = "Price", HeaderText = "Цена", DefaultCellStyle = { Format = "C2" } });
        dataGridView1.Columns.Add(new DataGridViewTextBoxColumn { Name = "QuantityInStock", DataPropertyName = "QuantityInStock", HeaderText = "В наличии", ReadOnly = true });
        
        var decreaseButton = new DataGridViewButtonColumn { Name = "Decrease", Text = "➖", UseColumnTextForButtonValue = true, Width = 40, HeaderText = "" };
        var increaseButton = new DataGridViewButtonColumn { Name = "Increase", Text = "➕", UseColumnTextForButtonValue = true, Width = 40, HeaderText = "" };
        var deleteButton = new DataGridViewButtonColumn { Name = "DeleteButton", HeaderText = "", Text = "❌", UseColumnTextForButtonValue = true, Width = 50 };
        
        dataGridView1.Columns.Add(decreaseButton);
        dataGridView1.Columns.Add(increaseButton);
        // ИСПРАВЛЕНО: Эта строка добавляет кнопку удаления в таблицу
        dataGridView1.Columns.Add(deleteButton);

        dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
    }

    private void LoadAllProducts()
    {
        ApplyFilters();
    }
    
    private void LoadCategories()
    {
        var categories = _productService.GetAllCategories();
        
        cmbCategory.Items.Clear();
        cmbCategory.Items.Add("Все категории");
        cmbCategory.Items.AddRange(categories.ToArray());
        cmbCategory.SelectedIndex = 0;
        
        cmbAddProductCategory.Items.Clear();
        cmbAddProductCategory.Items.AddRange(categories.ToArray());
    }
    
    private void ApplyFilters()
    {
        IEnumerable<Product> products = _productService.GetProducts();

        if (cmbCategory.SelectedItem != null && cmbCategory.SelectedItem.ToString() != "Все категории")
            products = products.Where(p => p.Category == cmbCategory.SelectedItem.ToString());

        if (!string.IsNullOrWhiteSpace(textBox1.Text))
            products = products.Where(p => p.ProductName.ToLower().Contains(textBox1.Text.ToLower()));

        if (checkBoxInStock.Checked)
            products = products.Where(p => p.QuantityInStock > 0);

        decimal priceFrom = NUDPriceFrom.Value;
        decimal priceTo = NUDPriceTo.Value == 0 ? decimal.MaxValue : NUDPriceTo.Value;
        if (priceTo >= priceFrom)
            products = products.Where(p => p.Price >= priceFrom && p.Price <= priceTo);

        if (rdButtonID.Checked) products = products.OrderBy(p => p.ProductId);
        else if (rdButtonName.Checked) products = products.OrderBy(p => p.ProductName);
        else if (rdButtonCheaper.Checked) products = products.OrderBy(p => p.Price);
        else if (radioButtonExpensive.Checked) products = products.OrderByDescending(p => p.Price);
        else if (rdButtonLittle.Checked) products = products.OrderBy(p => p.QuantityInStock);
        else if (rdButtonBig.Checked) products = products.OrderByDescending(p => p.QuantityInStock);

        dataGridView1.DataSource = new BindingList<Product>(products.ToList());
    }

    private void buttonAddProduct_Click(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(textBoxProductName.Text) || 
            string.IsNullOrWhiteSpace(textBoxProductArticle.Text) ||
            string.IsNullOrWhiteSpace(cmbAddProductCategory.Text))
        {
            MessageBox.Show("Все поля (Название, Артикул, Категория) должны быть заполнены.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (!decimal.TryParse(textBoxPrice.Text, out decimal price) || price < 0)
        {
            MessageBox.Show("Пожалуйста, введите корректную цену.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var newProduct = new Product
        {
            ProductName = textBoxProductName.Text.Trim(),
            ProductArticle = textBoxProductArticle.Text.Trim(),
            Category = cmbAddProductCategory.Text,
            Price = price,
            QuantityInStock = (int)NUDQuantity.Value
        };
        _productService.AddProduct(newProduct);
        
        textBoxProductName.Clear();
        textBoxProductArticle.Clear();
        textBoxPrice.Clear();
        NUDQuantity.Value = 1;
        cmbAddProductCategory.SelectedIndex = -1;
        
        ApplyFilters();
        LoadCategories();
        
        MessageBox.Show("Товар успешно добавлен!", "Успех", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
    
    private void DataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || dataGridView1.ReadOnly) return;
        
        var product = (Product)dataGridView1.Rows[e.RowIndex].DataBoundItem;
        string clickedColumnName = dataGridView1.Columns[e.ColumnIndex].Name;
        
        switch (clickedColumnName)
        {
            case "Increase": product.QuantityInStock++; _productService.UpdateProduct(product); break;
            case "Decrease": if (product.QuantityInStock > 0) { product.QuantityInStock--; _productService.UpdateProduct(product); } break;
            case "DeleteButton":
                var result = MessageBox.Show($"Вы уверены, что хотите удалить товар \"{product.ProductName}\"?", "Подтверждение удаления", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes) { _productService.DeleteProductById(product.ProductId); ApplyFilters(); }
                break;
        }
        dataGridView1.Refresh();
    }
    
    private void DataGridView1_CellValueChanged(object sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex >= 0 && dataGridView1.Columns[e.ColumnIndex] is DataGridViewTextBoxColumn)
        {
            var product = (Product)dataGridView1.Rows[e.RowIndex].DataBoundItem;
            _productService.UpdateProduct(product);
        }
    }
    
    private void buttonMenu_Click(object sender, EventArgs e)
    {
        this.Close();
        SellerMenuForm sellerMenuForm = new SellerMenuForm();
        sellerMenuForm.Show();
    }
}