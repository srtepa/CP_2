namespace course_project.Models;

public class Sale
{
    public int SaleId { get; set; }
    public DateTime SaleDate { get; set; }
    public string SellerUserName { get; set; }
    public decimal TotalAmount { get; set; }
    public List<SaleItem> Items { get; set; } = new List<SaleItem>();
}