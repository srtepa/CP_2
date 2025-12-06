using course_project.Models;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace course_project.Services;

public class ReportService
{
    private readonly string _filePath = "C:\\projects\\CP_2\\course_project\\course_project\\Sourse\\Z_Report.docx";
    private readonly string _reportsDirectory = "Reports";

    public string GenerateReport(Report reportData)
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException("Файл шаблона отчета не найден.", _filePath);
        }
            
        string fileName = $"Report_{reportData.Title.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.docx";
        string newFilePath = Path.Combine(_reportsDirectory, fileName);

        File.Copy(_filePath, newFilePath);

        using (var document = DocX.Load(newFilePath))
        {
            document.ReplaceText("{title}", reportData.Title ?? "Без названия");
            document.ReplaceText("{creationDate}", reportData.CreationDate.ToString("dd.MM.yyyy HH:mm"));
            document.ReplaceText("{createdByUser}", reportData.CreatedByUser ?? "Неизвестно");
                
            document.ReplaceText("{dateStart}", reportData.StartDate.ToString("dd.MM.yyyy"));
            document.ReplaceText("{dateEnd}", reportData.EndDate.ToString("dd.MM.yyyy"));

            document.ReplaceText("{revenue}", reportData.TotalRevenue.ToString("C")); // "C" форматирует как валюту
            document.ReplaceText("{salesCount}", reportData.TotalSalesCount.ToString());
            document.ReplaceText("{itemsCount}", reportData.TotalItemsSold.ToString());
            document.ReplaceText("{averageCheck}", reportData.AverageCheckValue.ToString("C"));

            //вставка таблицы с продажами
            var salesParagraph = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("{listSales}"));
            if (salesParagraph != null)
            {
                salesParagraph.RemoveText(0, salesParagraph.Text.Length);

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
                    salesParagraph.InsertTableAfterSelf(salesTable);
                }
                else
                {
                    salesParagraph.Append("Продажи за указанный период отсутствуют.").Bold();
                }
            }

            //вставка таблицы с популярными товарами
            var topProductsParagraph = document.Paragraphs.FirstOrDefault(p => p.Text.Contains("{bestSellingProducts}"));
            if (topProductsParagraph != null)
            {
                topProductsParagraph.RemoveText(0, topProductsParagraph.Text.Length);

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
                    topProductsParagraph.InsertTableAfterSelf(topProductsTable);
                }
                else
                {
                    topProductsParagraph.Append("Данные по популярным товарам отсутствуют.").Bold();
                }
            }
                
            document.Save();
        }

        return newFilePath; //возвращаем путь к созданному файлу
    }
}