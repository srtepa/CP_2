namespace course_project.Models;

public class Report
{
    public string Title { get; set; } //название отчета, например, "Отчет по продажам"
    public DateTime CreationDate { get; set; }
    public string CreatedByUser { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    
    //итоговые показатели
    public decimal TotalRevenue { get; set; }
    public int TotalSalesCount { get; set; }
    public int TotalItemsSold { get; set; }
    public decimal AverageCheckValue { get; set; }
    
    public List<Sale> Sales { get; set; } //список всех продаж, попавших в отчет
    public List<BestSellingProductInfo> TopSellingProducts { get; set; }
    
    public Report()
    {
        // Инициализируем списки, чтобы они не были null
        Sales = new List<Sale>();
        TopSellingProducts = new List<BestSellingProductInfo>();
    }
}