using course_project.Models;
using System.Text.Json;

namespace course_project.Services;

public class ProductService
{
    private readonly string _filePath = "C:\\projects\\CP_2\\course_project\\course_project\\Files\\Products.json";

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
        catch (Exception e)
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
        return _products.FirstOrDefault(p => p.ProductId == id);
    }

    public void AddProduct(Product product)
    {
        int newId = _products.Any() ? _products.Max(p => p.ProductId) + 1 : 1;
        product.ProductId = newId;

        _products.Add(product);
        SaveChanges();
    }

    public void UpdateProduct(Product product)
    {
        int index = _products.FindIndex(p => p.ProductId == product.ProductId);

        if (index != -1) // Если товар найден
        {
            _products[index] = product;
            SaveChanges();
        }
    }

    public void DeleteProductById(int id)
    {
        Product productToRemove = _products.FirstOrDefault(p => p.ProductId == id);
        if (productToRemove != null)
        {
            _products.Remove(productToRemove);
            SaveChanges();
        }
    }

    public List<string> GetAllCategories()
    {
        return _products.Select(p => p.Category).Distinct().OrderBy(c => c).ToList();
    }

    public List<Product> SearchProducts(string query, string category = null)
    {
        IEnumerable<Product> filteredProducts = _products.Where(p => p.QuantityInStock > 0);

        if (!string.IsNullOrWhiteSpace(category) && category != "Все категории")
        {
            filteredProducts = filteredProducts.Where(p =>
                (p.Category ?? "").Equals(category, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(query))
        {
            string lowerQuery = query.ToLower();
            filteredProducts = filteredProducts.Where(p =>
                (p.ProductName ?? "").ToLower().Contains(lowerQuery) ||
                (p.ProductArticle ?? "").ToLower().Contains(lowerQuery));
        }

        return filteredProducts.ToList();
    }

    public void ReduceQuantity(int productId, int quantity)
    {
        if (!File.Exists(_filePath)) return;

        string jsonData = File.ReadAllText(_filePath);
        List<Product> products = JsonSerializer.Deserialize<List<Product>>(jsonData);

        Product product = products.FirstOrDefault(p => p.ProductId == productId);

        if (product != null)
        {
            product.QuantityInStock -= quantity;
            if (product.QuantityInStock < 0) product.QuantityInStock = 0;

            SaveChanges();
        }
    }
}