using course_project.Models;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace course_project.Services;

public class ReportService
{
    private readonly string _templatePath;
    private readonly string _reportsDirectory;

    public ReportService()
    {
        // Динамическое определение путей, чтобы работало на любом ПК
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string projectRoot = Path.GetFullPath(Path.Combine(baseDirectory, @"..\..\.."));

        // Убедись, что файл шаблона называется именно так
        _templatePath = Path.Combine(projectRoot, "Sourse", "Report.docx"); 
        _reportsDirectory = Path.Combine(projectRoot, "Reports");
    }

    public string GenerateReport(Report reportData)
    {
        if (!File.Exists(_templatePath))
        {
            throw new FileNotFoundException($"Файл шаблона отчета не найден по пути: {_templatePath}");
        }
            
        if (!Directory.Exists(_reportsDirectory))
        {
            Directory.CreateDirectory(_reportsDirectory);
        }

        string fileName = $"Manager_Report_{reportData.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.docx";
        string newFilePath = Path.Combine(_reportsDirectory, fileName);

        // Копируем шаблон
        File.Copy(_templatePath, newFilePath, true);

        using (var document = DocX.Load(newFilePath))
        {
            // Замена простых полей
            document.ReplaceText("{title}", reportData.Title ?? "Отчет");
            document.ReplaceText("{creationDate}", DateTime.Now.ToString("dd.MM.yyyy HH:mm"));
            document.ReplaceText("{createdByUser}", reportData.CreatedByUser ?? "Менеджер");
            document.ReplaceText("{dateStart}", reportData.StartDate.ToString("dd.MM.yyyy"));
            document.ReplaceText("{dateEnd}", reportData.EndDate.ToString("dd.MM.yyyy")); // Исправлено на EndDate
            document.ReplaceText("{revenue}", reportData.TotalRevenue.ToString("C"));
            document.ReplaceText("{salesCount}", reportData.TotalSalesCount.ToString());
            document.ReplaceText("{itemsCount}", reportData.TotalItemsSold.ToString());
            document.ReplaceText("{averageCheck}", reportData.AverageCheckValue.ToString("C"));

            // 1. Таблица продаж
            var salesParagraph = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("{listSales}"));
            if (salesParagraph != null)
            {
                if (reportData.Sales != null && reportData.Sales.Any())
                {
                    var salesTable = document.InsertTable(reportData.Sales.Count + 1, 4);
                    salesTable.Design = TableDesign.TableGrid;

                    // Заголовки
                    salesTable.Rows[0].Cells[0].Paragraphs.First().Append("Дата");
                    salesTable.Rows[0].Cells[1].Paragraphs.First().Append("Продавец");
                    salesTable.Rows[0].Cells[2].Paragraphs.First().Append("Позиций");
                    salesTable.Rows[0].Cells[3].Paragraphs.First().Append("Сумма");

                    for (int i = 0; i < reportData.Sales.Count; i++)
                    {
                        var sale = reportData.Sales[i];
                        salesTable.Rows[i + 1].Cells[0].Paragraphs.First().Append(sale.SaleDate.ToString("g"));
                        salesTable.Rows[i + 1].Cells[1].Paragraphs.First().Append(sale.SellerUserName);
                        salesTable.Rows[i + 1].Cells[2].Paragraphs.First().Append(sale.Items.Sum(x => x.Quantity).ToString());
                        salesTable.Rows[i + 1].Cells[3].Paragraphs.First().Append(sale.TotalAmount.ToString("C"));
                    }
                    
                    salesParagraph.InsertTableBeforeSelf(salesTable);
                    salesParagraph.Remove(false);
                }
                else
                {
                    salesParagraph.ReplaceText("{listSales}", "Нет продаж за выбранный период.");
                }
            }

            // 2. Таблица популярных товаров
            var topProductsParagraph = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("{bestSellingProducts}"));
            if (topProductsParagraph != null)
            {
                if (reportData.TopSellingProducts != null && reportData.TopSellingProducts.Any())
                {
                    var topTable = document.InsertTable(reportData.TopSellingProducts.Count + 1, 4);
                    topTable.Design = TableDesign.TableGrid;

                    topTable.Rows[0].Cells[0].Paragraphs.First().Append("Артикул");
                    topTable.Rows[0].Cells[1].Paragraphs.First().Append("Товар");
                    topTable.Rows[0].Cells[2].Paragraphs.First().Append("Продано");
                    topTable.Rows[0].Cells[3].Paragraphs.First().Append("Выручка");

                    for (int i = 0; i < reportData.TopSellingProducts.Count; i++)
                    {
                        var prod = reportData.TopSellingProducts[i];
                        topTable.Rows[i + 1].Cells[0].Paragraphs.First().Append(prod.ProductArticle);
                        topTable.Rows[i + 1].Cells[1].Paragraphs.First().Append(prod.ProductName);
                        topTable.Rows[i + 1].Cells[2].Paragraphs.First().Append(prod.TotalQuantitySold.ToString());
                        topTable.Rows[i + 1].Cells[3].Paragraphs.First().Append(prod.TotalRevenueFromProduct.ToString("C"));
                    }
                    
                    topProductsParagraph.InsertTableBeforeSelf(topTable);
                    topProductsParagraph.Remove(false);
                }
                else
                {
                    topProductsParagraph.ReplaceText("{bestSellingProducts}", "Нет данных.");
                }
            }
                
            document.Save();
        }

        return newFilePath;
    }
}