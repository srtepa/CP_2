namespace course_project.Models;

//вспомогательная модель для отображения позиций в DataGridView
public class SaleItemDisplay
{
    public int ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductArticle { get; set; }
    public int Quantity { get; set; }
    public decimal PriceAtTimeOfSale { get; set; }
    public decimal TotalItemAmount => Quantity * PriceAtTimeOfSale;
}