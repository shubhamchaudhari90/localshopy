using localshopyNew.Models;
using System.Text.Json;

namespace localshopyNew.Services
{
    public class CategoryService
    {
        private readonly string _filePath;

        public CategoryService(IWebHostEnvironment env)
        {
            _filePath = Path.Combine(env.ContentRootPath, "App_Data", "Products.json");
        }

        private List<ProductMaster> ReadFile()
        {
            if (!File.Exists(_filePath))
                return new List<ProductMaster>();

            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<List<ProductMaster>>(json) ?? new();
        }

        private void WriteFile(List<ProductMaster> categories)
        {
            var json = JsonSerializer.Serialize(categories, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(_filePath, json);
        }

        // READ
        public List<ProductMaster> GetAll() => ReadFile();

        public List<string> GetAllProducts()
        {
            var categories = GetAll();

            var products = categories
                .SelectMany(c => c.ProductName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            return products;
        }

        public ProductMaster? GetById(int id) =>
            ReadFile().FirstOrDefault(c => c.Id == id);

        // CREATE
        public void Add(ProductMaster category)
        {
            var categories = ReadFile();
            category.Id = categories.Any() ? categories.Max(c => c.Id) + 1 : 1;
            categories.Add(category);
            WriteFile(categories);
        }

        // UPDATE
        public void Update(ProductMaster category)
        {
            var categories = ReadFile();
            var existing = categories.FirstOrDefault(c => c.Id == category.Id);
            if (existing == null) return;

            existing.CategoryName = category.CategoryName;
            existing.ProductName = category.ProductName;

            WriteFile(categories);
        }

        // DELETE
        public void Delete(int id)
        {
            var categories = ReadFile();
            var category = categories.FirstOrDefault(c => c.Id == id);
            if (category == null) return;

            categories.Remove(category);
            WriteFile(categories);
        }
    }
}
