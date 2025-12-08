using System.Diagnostics;
using course_project.Models;
using course_project.Services;

namespace course_project.Forms
{
    public partial class ManagerMenuForm : Form
    {
        private readonly SaleService _saleService;
        private readonly ProductService _productService;
        private readonly ZReportService _zReportService;

        public ManagerMenuForm()
        {
            InitializeComponent();
            
            _saleService = new SaleService();
            _productService = new ProductService();
            _zReportService = new ZReportService();
        }

        private void buttonSale_Click(object sender, EventArgs e)
        {
            this.Hide();
            SaleForm saleForm = new SaleForm();
            saleForm.Show();
        }

        private void buttonProducts_Click(object sender, EventArgs e)
        {
            this.Hide();
            ProductsForm productsForm = new ProductsForm();
            productsForm.Show();
        }

        private void buttonHistory_Click(object sender, EventArgs e)
        {
            this.Hide();
            ManagerSalesHistoryForm historyForm = new ManagerSalesHistoryForm();
            historyForm.Show();
        }
        
        private void buttonStats_Click(object sender, EventArgs e)
        {
            this.Hide();
            ManagerStatisticsForm stats = new ManagerStatisticsForm();
            stats.Show();
        }

        // Отчет за смену (день)
        private void buttonRepDay_Click(object sender, EventArgs e)
        {
            DateTime dateStart = DateTime.Today;
            DateTime dateEnd = DateTime.Today.AddDays(1).AddTicks(-1);
            
            List<Sale> salesForToday = _saleService.GetAllSaleForPeriod(dateStart, dateEnd);
            
            if (!salesForToday.Any())
            {
                MessageBox.Show("За сегодня еще не было ни одной продажи.", "Нет данных", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return; 
            }

            var reportData = new Report
            {
                Title = "Отчет по продажам (Смена)",
                CreationDate = DateTime.Now,
                CreatedByUser = SessionManager.CurrentUser?.UserName ?? "Менеджер",
                StartDate = dateStart,
                EndDate = dateEnd,
                TotalSalesCount = salesForToday.Count,
                TotalRevenue = salesForToday.Sum(s => s.TotalAmount),
                TotalItemsSold = salesForToday.SelectMany(s => s.Items).Sum(i => i.Quantity),
                AverageCheckValue = salesForToday.Any() ? salesForToday.Average(s => s.TotalAmount) : 0,
                Sales = salesForToday 
            };
            
            reportData.TopSellingProducts = salesForToday
                .SelectMany(sale => sale.Items)
                .GroupBy(item => item.ProductId)
                .Select(group => {
                    var product = _productService.GetProductById(group.Key);
                    return new BestSellingProductInfo
                    {
                        ProductArticle = product?.ProductArticle ?? "N/A",
                        ProductName = product?.ProductName ?? "Удаленный товар",
                        TotalQuantitySold = group.Sum(item => item.Quantity),
                        TotalRevenueFromProduct = group.Sum(item => item.PriceAtTimeOfSale * item.Quantity)
                    };
                })
                .OrderByDescending(info => info.TotalQuantitySold)
                .Take(5)
                .ToList();

            try
            {
                buttonRepDay.Enabled = false;

                string filePath = _zReportService.GenerateReport(reportData);

                var result = MessageBox.Show(
                    $"Отчет за смену успешно создан!\n\nФайл: {filePath}\n\nОткрыть?",
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
                buttonRepDay.Enabled = true;
            }
        }

        // Отчет за период (открывает форму настройки)
        private void buttonRepTime_Click(object sender, EventArgs e)
        {
            this.Hide();
            ManagerReportForm reportForm = new ManagerReportForm();
            reportForm.Show();
        }

        private void buttonReEntry_Click(object sender, EventArgs e)
        {
            this.Close();
            AuthForm authForm = new AuthForm();
            authForm.Show();
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }
    }
}