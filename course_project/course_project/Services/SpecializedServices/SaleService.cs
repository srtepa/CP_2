using System.Text.Json;
using course_project.Models;

namespace course_project.Services;

public class SaleService
{
    // Динамический путь, чтобы работало на любом компьютере
    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\..\..\Files\Sales.json");
    
    private List<Sale> _sales;

    public SaleService()
    {
        LoadSales();
    }

    private void LoadSales()
    {
        try
        {
            // Используем Path.GetFullPath для корректной работы с относительным путем
            string fullPath = Path.GetFullPath(_filePath);

            if (!File.Exists(fullPath))
            {
                _sales = new List<Sale>();
                // Создаем файл, если его нет
                SaveChanges(); 
                return;
            }
            
            string jsonData = File.ReadAllText(fullPath);
            
            if (string.IsNullOrWhiteSpace(jsonData))
            {
                _sales = new List<Sale>();
                return;
            }
            
            _sales = JsonSerializer.Deserialize<List<Sale>>(jsonData);
        }
        catch (JsonException ex)
        {
            MessageBox.Show($"Ошибка формата в файле истории продаж: {ex.Message}", "Ошибка чтения", MessageBoxButtons.OK, MessageBoxIcon.Error);
            _sales = new List<Sale>();
        }
        catch (Exception e)
        {
            MessageBox.Show($"Произошла ошибка при загрузке истории продаж: {e.Message}");
            _sales = new List<Sale>();
        }
    }

    private void SaveChanges()
    {
        try
        {
            string fullPath = Path.GetFullPath(_filePath);
            
            // Убедимся, что папка существует
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var options = new JsonSerializerOptions { WriteIndented = true };
            string jsonData = JsonSerializer.Serialize(_sales, options);
            File.WriteAllText(fullPath, jsonData);
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

    // ИСПРАВЛЕННЫЙ МЕТОД: Теперь он не падает, если передать максимальную дату
    public List<Sale> GetAllSaleForPeriod(DateTime startDate, DateTime endDate)
    {
        // Проверяем, не является ли дата максимальной, чтобы не получить ошибку при добавлении дня
        if (endDate.Date < DateTime.MaxValue.Date)
        {
            // Берем конец указанного дня (23:59:59...)
            endDate = endDate.Date.AddDays(1).AddTicks(-1);
        }
        
        return _sales.Where(s => s.SaleDate >= startDate && s.SaleDate <= endDate).ToList();
    }

    public int GetNextSaleId()
    {
        return _sales.Any() ? _sales.Max(s => s.SaleId) + 1 : 1;
    }

    public void AddSale(Sale sale)
    {
        sale.SaleId = GetNextSaleId();
        _sales.Add(sale);
        SaveChanges();
    }
}