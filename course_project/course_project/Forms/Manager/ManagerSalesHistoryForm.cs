using course_project.Models;
using course_project.Services;
using System.Data;

namespace course_project.Forms;

public partial class ManagerSalesHistoryForm : Form
{
    private readonly SaleService _saleService;
    private readonly ProductService _productService;
    private readonly UserService _userService; // Для загрузки продавцов
    private List<Sale> _allSales;

    public ManagerSalesHistoryForm()
    {
        InitializeComponent();
        
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        _saleService = new SaleService();
        _productService = new ProductService();
        _userService = new UserService();
    }

    private void SalesHistoryForm_Load(object sender, EventArgs e)
    {
        SetupGrids();
        LoadSellers();
        LoadAllSales(); // Загружаем все продажи в память
        
        // Устанавливаем значения фильтров по умолчанию
        ResetFilters();
        
        // Привязываем события кнопок фильтрации
        btnApply.Click += (s, ev) => ApplyFilters();
        btnReset.Click += (s, ev) => ResetFilters();
        
        // Применяем фильтры сразу при старте (покажет всё, отсортированное по дате)
        ApplyFilters();
    }

    private void SetupGrids()
    {
        // Левая таблица (Продажи)
        dgvSales.AutoGenerateColumns = false;
        dgvSales.Columns.Clear();
        dgvSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SaleId", HeaderText = "ID", Width = 60 });
        dgvSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SaleDate", HeaderText = "Дата и время", Width = 150 });
        dgvSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "SellerUserName", HeaderText = "Продавец", Width = 120 });
        dgvSales.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalAmount", HeaderText = "Сумма", DefaultCellStyle = { Format = "C2" } });

        // Правая таблица (Товары)
        dgvDetails.AutoGenerateColumns = false;
        dgvDetails.Columns.Clear();
        dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductArticle", HeaderText = "Артикул", Width = 100 });
        dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "ProductName", HeaderText = "Товар", Width = 250 });
        dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Quantity", HeaderText = "Кол-во", Width = 70 });
        dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PriceAtTimeOfSale", HeaderText = "Цена", DefaultCellStyle = { Format = "C2" }, Width = 100 });
        dgvDetails.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "TotalItemAmount", HeaderText = "Сумма", DefaultCellStyle = { Format = "C2" } });
    }

    private void LoadSellers()
    {
        cmbSellers.Items.Clear();
        cmbSellers.Items.Add("Все");
        
        var users = _userService.GetUsers();
        foreach (var user in users)
        {
            cmbSellers.Items.Add(user.UserName);
        }
        cmbSellers.SelectedIndex = 0;
    }

    private void LoadAllSales()
    {
        // Загружаем вообще все продажи за всё время в память
        // Используем большой диапазон дат, чтобы захватить всё
        DateTime minDate = DateTime.MinValue;
        DateTime maxDate = DateTime.Now.AddDays(1); 
        
        _allSales = _saleService.GetAllSaleForPeriod(minDate, maxDate);
    }

    private void ResetFilters()
    {
        // Сброс значений UI
        dtpStart.Value = DateTime.Now.AddMonths(-1); // По умолчанию - последний месяц
        dtpEnd.Value = DateTime.Now;
        cmbSellers.SelectedIndex = 0;
        nudMinSum.Value = 0;
        nudMaxSum.Value = 0;
        nudSaleId.Value = 0;
        rbDateNew.Checked = true;
        
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (_allSales == null) return;

        IEnumerable<Sale> filteredSales = _allSales;

        // 1. Фильтр по ID (если задан, остальные фильтры игнорируем или применяем вместе - тут сделаем "вместе")
        if (nudSaleId.Value > 0)
        {
            filteredSales = filteredSales.Where(s => s.SaleId == (int)nudSaleId.Value);
        }
        else
        {
            // 2. Фильтр по дате
            // Важно: берем начало дня Start и конец дня End
            DateTime start = dtpStart.Value.Date;
            DateTime end = dtpEnd.Value.Date.AddDays(1).AddTicks(-1);
            
            filteredSales = filteredSales.Where(s => s.SaleDate >= start && s.SaleDate <= end);

            // 3. Фильтр по продавцу
            if (cmbSellers.SelectedItem != null && cmbSellers.SelectedItem.ToString() != "Все")
            {
                filteredSales = filteredSales.Where(s => s.SellerUserName == cmbSellers.SelectedItem.ToString());
            }

            // 4. Фильтр по сумме
            decimal minSum = nudMinSum.Value;
            decimal maxSum = nudMaxSum.Value;

            if (maxSum > 0)
            {
                filteredSales = filteredSales.Where(s => s.TotalAmount >= minSum && s.TotalAmount <= maxSum);
            }
            else if (minSum > 0)
            {
                filteredSales = filteredSales.Where(s => s.TotalAmount >= minSum);
            }
        }

        // 5. Сортировка
        if (rbDateNew.Checked)
            filteredSales = filteredSales.OrderByDescending(s => s.SaleDate);
        else if (rbDateOld.Checked)
            filteredSales = filteredSales.OrderBy(s => s.SaleDate);
        else if (rbSumAsc.Checked)
            filteredSales = filteredSales.OrderBy(s => s.TotalAmount);
        else if (rbSumDesc.Checked)
            filteredSales = filteredSales.OrderByDescending(s => s.TotalAmount);

        // Обновляем таблицу
        dgvSales.DataSource = filteredSales.ToList();
    }

    private void dgvSales_SelectionChanged(object sender, EventArgs e)
    {
        if (dgvSales.SelectedRows.Count == 0) 
        {
            dgvDetails.DataSource = null;
            return;
        }

        Sale selectedSale = (Sale)dgvSales.SelectedRows[0].DataBoundItem;

        var displayItems = selectedSale.Items.Select(item => 
        {
            var product = _productService.GetProductById(item.ProductId);
            return new SaleItemDisplay
            {
                ProductId = item.ProductId,
                ProductName = product?.ProductName ?? "Удаленный товар",
                ProductArticle = product?.ProductArticle ?? "N/A",
                Quantity = item.Quantity,
                PriceAtTimeOfSale = item.PriceAtTimeOfSale
            };
        }).ToList();

        dgvDetails.DataSource = displayItems;
    }

    private void btnBack_Click(object sender, EventArgs e)
    {
        this.Close();
        ManagerMenuForm menu = new ManagerMenuForm();
        menu.Show();
    }
}