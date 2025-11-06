namespace course_project.Models;

internal class Product
{
    public int ProductId { get; set; }
    public string ProductArticle { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int QuantityInStok { get; set; }
    
}