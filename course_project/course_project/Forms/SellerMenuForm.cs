using System.Diagnostics;
using course_project.Models;
using course_project.Services;

namespace course_project.Forms
{
    public partial class SellerMenuForm : Form
    {
        private readonly SaleService _saleService;
        private readonly ProductService _productService;
        private readonly ZReportService _zReportService;

        public SellerMenuForm()
        {
            InitializeComponent();
            
            _saleService = new SaleService();
            _productService = new ProductService();
            _zReportService = new ZReportService();

            // ВАЖНО: Убедитесь, что здесь НЕТ ручной привязки событий для кнопок,
            // так как они уже привязаны в файле Designer.cs.
            // Например, здесь НЕ должно быть строки:
            // this.buttonReport.Click += new System.EventHandler(this.buttonReport_Click);
        }

        private void CashierForm_Load(object sender, EventArgs e)
        {
            // Этот метод можно оставить пустым или удалить, если он не используется
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void buttonUpdateUser_Click(object sender, EventArgs e)
        {
            this.Close();
            AuthForm authForm = new AuthForm();
            authForm.Show();
        }

        private void buttonSale_Click(object sender, EventArgs e)
        {
            this.Close();
            SellerSaleForm saleForm = new SellerSaleForm();
            saleForm.Show();
        }

        private void buttonProducts_Click(object sender, EventArgs e)
        {
            this.Close();
            ProductsForm productsForm = new ProductsForm();
            productsForm.Show();
        }

        // Этот метод вызывается ОДИН РАЗ благодаря привязке в Designer.cs
        private void buttonReport_Click(object sender, EventArgs e)
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
                Title = "Отчет по продажам.",
                CreationDate = DateTime.Now,
                CreatedByUser = SessionManager.CurrentUser.UserName ?? "Система",
                StartDate = dateStart,
                EndDate = dateEnd,
                TotalSalesCount = salesForToday.Count,
                TotalRevenue = salesForToday.Sum(s => s.TotalAmount),
                TotalItemsSold = salesForToday.SelectMany(s => s.Items).Sum(i => i.Quantity),
                AverageCheckValue = salesForToday.Any() ? salesForToday.Average(s => s.TotalAmount) : 0,
                // Эта строка передает все продажи для отчета
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
                buttonReport.Enabled = false;

                string filePath = _zReportService.GenerateReport(reportData);

                var result = MessageBox.Show(
                    $"Z-Отчет за сегодня успешно создан!\n\nФайл сохранен здесь:\n{filePath}\n\nХотите открыть его сейчас?",
                    "Генерация отчета завершена",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при создании отчета: {ex.Message}", "Критическая ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                buttonReport.Enabled = true;
            }
        }
    }
}