using course_project.Models;
using System.Text.Json;

namespace course_project.Services;

internal class ProductService
{
    private readonly string _filePath = "C:\\Users\\stepankonon\\Documents\\CP_2\\course_project\\course_project\\Files\\Products.json";
    
    private List<Product> _products;

    public ProductService()
    {
        LoadProducts();
    }

    private void LoadProducts()
    {
        try
        {
            if (!File.Exists(_filePath))
            {
                _products = new List<Product>();
                return;
            }

            string jsonData = File.ReadAllText(_filePath);
            _products = JsonSerializer.Deserialize<List<Product>>(jsonData);
        }
        catch(Exception e)
        {
            MessageBox.Show($"Ошибка при загрузке товаров: {e.Message}");
            _products = new List<Product>();
        }
    }

    private void SaveChanges()
    {
        try
        {
            //настраиваем параметры сериализации для красивого вывода (с отступами).
            var options = new JsonSerializerOptions
            {
                WriteIndented = true // Делает JSON-файл читаемым для человека
            };
            
            string jsonData = JsonSerializer.Serialize<List<Product>>(_products, options);
            File.WriteAllText(_filePath, jsonData);
        }
        catch (Exception e)
        {
            MessageBox.Show($"Ошибка при сохранении товаров: {e.Message}");
        }
    }

    public List<Product> GetProducts()
    {
        return _products;
    }

    public Product GetProductById(int id)
    {
        return _products.FirstOrDefault(p => p.Id == id);
    }

    public void AddProduct(Product product)
    {
        int newId = _products.Any() ? _products.Max(p => p.Id) + 1 : 1;
        product.Id = newId;
        
        _products.Add(product);
        SaveChanges();
    }

    public void UpdateProduct(Product product)
    {
        int index = _products.FindIndex(p => p.Id == product.Id);
        
        if (index != -1) // Если товар найден
        {
            _products[index] = product;
            SaveChanges();
        }
    }

    public void DeleteProductById(int id)
    {
        Product productToRemove = _products.FirstOrDefault(p => p.Id == id);
        if (productToRemove != null)
        {
            _products.Remove(productToRemove);
            SaveChanges();
        }
    }
}