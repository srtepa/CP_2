using System.Text.Json;
using course_project.Models;

namespace course_project.Services;

internal class SaleService
{
    private readonly string _filePath =
        "C:\\Users\\stepankonon\\Documents\\CP_2\\course_project\\course_project\\Files\\Sales.json";
    private List<Sale> _sales;

    private SaleService()
    {
        LoadSales();
    }

    private void LoadSales()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _sales = new List<Sale>();
                SaveChanges();
            }

            string jsonData = File.ReadAllText(_filePath);
            _sales = JsonSerializer.Deserialize<List<Sale>>(jsonData);
        }
        catch (Exception e)
        {
            MessageBox.Show($"Ошибка при загрузке истории продаж: {e.Message}");
            _sales = new List<Sale>();
        }
    }

    private void SaveChanges()
    {
        try
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonData = JsonSerializer.Serialize(_sales, options);
            File.WriteAllText(_filePath, jsonData);
        }
        catch (Exception e)
        {
            MessageBox.Show($"Ошибка при сохранении продажи: {e.Message}");
        }
    }

    public List<Sale> GetAllSales()
    {
        return _sales;
    }

    public List<Sale> GetAllSaleForPeriod(DateTime startDate, DateTime endDate)
    {
        endDate = endDate.Date.AddDays(1).AddTicks(-1);
        return _sales.Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate).ToList();
    }

    public int GetNextSaleId()
    {
        return _sales.Any() ? _sales.Max(s => s.SaleId) + 1 : 0;
    }

    public void AddSale(Sale sale)
    {
        sale.SaleId = GetNextSaleId();
        _sales.Add(sale);
        SaveChanges();
    }
}