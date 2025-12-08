using System.Diagnostics;
using course_project.Models;
using course_project.Services;

namespace course_project.Forms;

public partial class ManagerReportForm : Form
{
    private readonly SaleService _saleService;
    private readonly UserService _userService;
    private readonly ProductService _productService;
    private readonly ReportService _reportService;

    public ManagerReportForm()
    {
        InitializeComponent();
        
        _saleService = new SaleService();
        _userService = new UserService();
        _productService = new ProductService();
        _reportService = new ReportService();
        
        // Отключаем изменение размера для красоты
        this.FormBorderStyle = FormBorderStyle.FixedSingle;
        this.MaximizeBox = false;
    }

    private void ManagerReportForm_Load(object sender, EventArgs e)
    {
        LoadUsers();
        
        // Устанавливаем даты по умолчанию (начало месяца - текущий момент)
        DateTime now = DateTime.Now;
        dtpStart.Value = new DateTime(now.Year, now.Month, 1);
        dtpEnd.Value = now;
    }

    private void LoadUsers()
    {
        cmbUsers.Items.Clear();
        
        // Опция для отчета по всем сразу
        cmbUsers.Items.Add("Все пользователи");
        
        // Загружаем список пользователей из сервиса
        var users = _userService.GetUsers();
        foreach (var user in users)
        {
            cmbUsers.Items.Add(user.UserName);
        }
        
        cmbUsers.SelectedIndex = 0; // Выбираем "Все пользователи" по умолчанию
    }

    private void btnGenerate_Click(object sender, EventArgs e)
    {
        // 1. Получаем период. Важно: для конца периода берем конец дня (23:59:59)
        DateTime startDate = dtpStart.Value.Date;
        DateTime endDate = dtpEnd.Value.Date.AddDays(1).AddTicks(-1);

        if (startDate > endDate)
        {
            MessageBox.Show("Дата начала не может быть позже даты окончания.", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        // 2. Получаем все продажи за период
        var sales = _saleService.GetAllSaleForPeriod(startDate, endDate);

        // 3. Фильтрация по пользователю
        string selectedUser = cmbUsers.SelectedItem.ToString();
        if (selectedUser != "Все пользователи")
        {
            sales = sales.Where(s => s.SellerUserName == selectedUser).ToList();
        }

        if (!sales.Any())
        {
            MessageBox.Show($"За период с {startDate:d} по {endDate:d} продаж не найдено (Пользователь: {selectedUser}).", "Нет данных", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        // 4. Формируем объект отчета
        var reportData = new Report
        {
            Title = txtReportTitle.Text,
            CreationDate = DateTime.Now,
            CreatedByUser = SessionManager.CurrentUser.UserName, // Текущий менеджер
            StartDate = startDate,
            EndDate = endDate, // Используем выбранную дату конца
            
            // Статистика
            TotalRevenue = sales.Sum(s => s.TotalAmount),
            TotalSalesCount = sales.Count,
            TotalItemsSold = sales.SelectMany(s => s.Items).Sum(i => i.Quantity),
            AverageCheckValue = sales.Average(s => s.TotalAmount),
            
            Sales = sales // Передаем отфильтрованные продажи
        };

        // 5. Считаем популярные товары на основе отфильтрованных продаж
        reportData.TopSellingProducts = sales
            .SelectMany(sale => sale.Items)
            .GroupBy(item => item.ProductId)
            .Select(group => {
                var product = _productService.GetProductById(group.Key);
                return new BestSellingProductInfo
                {
                    ProductArticle = product?.ProductArticle ?? "N/A",
                    ProductName = product?.ProductName ?? "Удален",
                    TotalQuantitySold = group.Sum(item => item.Quantity),
                    TotalRevenueFromProduct = group.Sum(item => item.PriceAtTimeOfSale * item.Quantity)
                };
            })
            .OrderByDescending(info => info.TotalQuantitySold)
            .Take(10) // Топ 10, например
            .ToList();

        // 6. Генерируем документ
        try
        {
            btnGenerate.Enabled = false;
            string filePath = _reportService.GenerateReport(reportData);

            var result = MessageBox.Show(
                $"Отчет успешно создан!\nСохранен: {filePath}\n\nОткрыть файл?",
                "Успех", MessageBoxButtons.YesNo, MessageBoxIcon.Information);

            if (result == DialogResult.Yes)
            {
                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            btnGenerate.Enabled = true;
        }
    }

    private void btnBack_Click(object sender, EventArgs e)
    {
        this.Close();
        ManagerMenuForm menu = new ManagerMenuForm();
        menu.Show();
    }
}