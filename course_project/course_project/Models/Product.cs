using System.ComponentModel;

namespace course_project.Models;

public class Product
{
    public int ProductId { get; set; }
    public string ProductArticle { get; set; }
    public string ProductName { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    [DisplayName("В наличии")]
    public decimal QuantityInStock { get; set; }
    
}