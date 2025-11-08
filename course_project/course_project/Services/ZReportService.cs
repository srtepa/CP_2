using course_project.Models;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace course_project.Services
{
    public class ZReportService
    {
        private readonly string _templatePath;
        private readonly string _reportsDirectory;

        // Конструктор сервиса, который вычисляет правильные пути к папкам проекта
        public ZReportService()
        {
            // 1. Получаем путь к папке, где запущена программа ( ...\bin\Debug\net8.0-windows\ )
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

            // 2. "Поднимаемся" на 3 уровня вверх, чтобы попасть в корневую папку вашего проекта
            string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\.."));

            // 3. Формируем полные и правильные пути к папкам Sourse и Reports внутри вашего проекта
            _templatePath = Path.Combine(projectRoot, "Sourse", "Z_Report.docx");
            _reportsDirectory = Path.Combine(projectRoot, "Reports");
        }

        public string GenerateReport(Report reportData)
        {
            // Проверяем, на месте ли шаблон
            if (!File.Exists(_templatePath))
            {
                throw new FileNotFoundException($"Файл шаблона отчета не найден по пути: {_templatePath}");
            }
            
            // Проверяем, существует ли папка для отчетов. Если нет - создаем ее.
            if (!Directory.Exists(_reportsDirectory))
            {
                Directory.CreateDirectory(_reportsDirectory);
            }

            // Создаем уникальное имя для нового файла отчета
            string fileName = $"Report_{reportData.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.docx";
            string newFilePath = Path.Combine(_reportsDirectory, fileName);

            // Копируем шаблон в новый файл
            File.Copy(_templatePath, newFilePath, true);

            // Открываем новый документ для работы
            using (var document = DocX.Load(newFilePath))
            {
                // Заменяем простой текст (плейсхолдеры)
                document.ReplaceText("{title}", reportData.Title ?? "Без названия");
                document.ReplaceText("{creationDate}", reportData.CreationDate.ToString("dd.MM.yyyy HH:mm"));
                document.ReplaceText("{createdByUser}", reportData.CreatedByUser ?? "Неизвестно");
                document.ReplaceText("{date}", reportData.StartDate.ToString("dd.MM.yyyy"));
                document.ReplaceText("{revenue}", reportData.TotalRevenue.ToString("C"));
                document.ReplaceText("{salesCount}", reportData.TotalSalesCount.ToString());
                document.ReplaceText("{itemsCount}", reportData.TotalItemsSold.ToString());
                document.ReplaceText("{averageCheck}", reportData.AverageCheckValue.ToString("C"));
                
                // Вставка таблицы с продажами
                var salesParagraph = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("{listSales}"));
                if (salesParagraph != null)
                {
                    if (reportData.Sales.Any())
                    {
                        var salesTable = document.InsertTable(reportData.Sales.Count + 1, 4);
                        salesTable.Design = TableDesign.TableGrid;
                        salesTable.Rows[0].Cells[0].Paragraphs.First().Append("Дата продажи");
                        salesTable.Rows[0].Cells[1].Paragraphs.First().Append("Продавец");
                        salesTable.Rows[0].Cells[2].Paragraphs.First().Append("Кол-во товаров");
                        salesTable.Rows[0].Cells[3].Paragraphs.First().Append("Сумма");
                        for (int i = 0; i < reportData.Sales.Count; i++)
                        {
                            var sale = reportData.Sales[i];
                            salesTable.Rows[i + 1].Cells[0].Paragraphs.First().Append(sale.SaleDate.ToString("g"));
                            salesTable.Rows[i + 1].Cells[1].Paragraphs.First().Append(sale.SellerUserName);
                            salesTable.Rows[i + 1].Cells[2].Paragraphs.First().Append(sale.Items.Sum(item => item.Quantity).ToString());
                            salesTable.Rows[i + 1].Cells[3].Paragraphs.First().Append(sale.TotalAmount.ToString("C"));
                        }
                        salesParagraph.InsertTableBeforeSelf(salesTable); // Вставляем таблицу ДО плейсхолдера
                        salesParagraph.Remove(false); // Удаляем плейсхолдер
                    }
                    else
                    {
                        salesParagraph.ReplaceText("{listSales}", "Продажи за указанный период отсутствуют.");
                    }
                }

                // Вставка таблицы с популярными товарами
                var topProductsParagraph = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("{bestSellingProducts}"));
                if (topProductsParagraph != null)
                {
                    if (reportData.TopSellingProducts.Any())
                    {
                        var topProductsTable = document.InsertTable(reportData.TopSellingProducts.Count + 1, 4);
                        topProductsTable.Design = TableDesign.TableGrid;
                        topProductsTable.Rows[0].Cells[0].Paragraphs.First().Append("Артикул");
                        topProductsTable.Rows[0].Cells[1].Paragraphs.First().Append("Название товара");
                        topProductsTable.Rows[0].Cells[2].Paragraphs.First().Append("Продано (шт)");
                        topProductsTable.Rows[0].Cells[3].Paragraphs.First().Append("Выручка");
                        for (int i = 0; i < reportData.TopSellingProducts.Count; i++)
                        {
                            var productInfo = reportData.TopSellingProducts[i];
                            topProductsTable.Rows[i + 1].Cells[0].Paragraphs.First().Append(productInfo.ProductArticle);
                            topProductsTable.Rows[i + 1].Cells[1].Paragraphs.First().Append(productInfo.ProductName);
                            topProductsTable.Rows[i + 1].Cells[2].Paragraphs.First().Append(productInfo.TotalQuantitySold.ToString());
                            topProductsTable.Rows[i + 1].Cells[3].Paragraphs.First().Append(productInfo.TotalRevenueFromProduct.ToString("C"));
                        }
                        topProductsParagraph.InsertTableBeforeSelf(topProductsTable); // Вставляем таблицу ДО плейсхолдера
                        topProductsParagraph.Remove(false); // Удаляем плейсхолдер
                    }
                    else
                    {
                        topProductsParagraph.ReplaceText("{bestSellingProducts}", "Данные по популярным товарам отсутствуют.");
                    }
                }
                
                // Сохраняем все изменения в документе
                document.Save();
            }

            // Возвращаем полный путь к созданному файлу
            return newFilePath;
        }
    }
}