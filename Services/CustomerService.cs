using localshopyNew.Models;
using System.Text.Json;

namespace localshopyNew.Services
{
    public class CustomerService
    {
        private readonly string _locationsPath;
        private readonly string _shopFolder;
        private readonly string _productsPath;

        public CustomerService(IWebHostEnvironment env)
        {
            _shopFolder = Path.Combine(env.ContentRootPath, "App_Data", "Shops");
            _locationsPath = Path.Combine(env.ContentRootPath, "App_Data", "Locations.json");
            _productsPath = Path.Combine(env.ContentRootPath, "App_Data", "Products.json");
            Directory.CreateDirectory(_shopFolder);
        }

        public List<CustomerProductViewModel> GetAllProducts(string? location)
        {
            var allProducts = new List<CustomerProductViewModel>();
            if (string.IsNullOrEmpty(location))
            {
                return allProducts;
            }
            var files = Directory.GetFiles(_shopFolder, "*.json");
            var categories = ReadCategoryJson();

            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var shop = JsonSerializer.Deserialize<Shop>(json);

                if (shop == null || !shop.IsOpen)
                    continue;

                if (shop.ServedLocations.Contains(location, StringComparer.OrdinalIgnoreCase))
                {
                    foreach (var product in shop.Products.Where(p => p.IsAvailable))
                    {
                        allProducts.Add(new CustomerProductViewModel
                        {
                            ShopName = shop.Name,
                            ShopPhoneNo = shop.PhoneNo,
                            Product = product,
                            CategotyName = GetCategoryByProduct(categories, product.Name),
                        });
                    }
                }
            }
            return [.. allProducts.OrderBy(x => x.CategotyName)];
        }

        public List<CustomerProductViewModel> GetProductsByShop(string shopName)
        {
            var allProducts = new List<CustomerProductViewModel>();
            var files = Directory.GetFiles(_shopFolder, $"{shopName}.json");
            var categories = ReadCategoryJson();

            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var shop = JsonSerializer.Deserialize<Shop>(json);

                if (shop == null)
                    return allProducts;

                foreach (var product in shop.Products.Where(p => p.IsAvailable))
                {
                    allProducts.Add(new CustomerProductViewModel
                    {
                        ShopName = shop.Name,
                        ShopPhoneNo = shop.PhoneNo,
                        Product = product,
                        CategotyName = GetCategoryByProduct(categories, product.Name),
                    });
                }
            }
            return [.. allProducts.OrderBy(x => x.CategotyName)];
        }

        public List<string> GetAllLocations()
        {
            var locations = ReadLocationsJson().Locations;
            return locations;
        }

        public bool IsLocationValid(string location)
        {
            var locations = ReadLocationsJson().Locations;
            if (locations.Contains(location, StringComparer.OrdinalIgnoreCase))
                return true;
            return false;
        }

        private LocationsData ReadLocationsJson()
        {
            if (!File.Exists(_locationsPath))
                return new LocationsData();

            var json = File.ReadAllText(_locationsPath);
            return JsonSerializer.Deserialize<LocationsData>(json) ?? new LocationsData();

        }

        private List<Category> ReadCategoryJson()
        {
            if (!File.Exists(_productsPath))
                return [];

            var json = File.ReadAllText(_productsPath);

            return JsonSerializer.Deserialize<List<Category>>(json) ?? [];
        }

        public string GetCategoryByProduct(List<Category> categories, string productName)
        {
            string? category = categories.FirstOrDefault(c => c.ProductName.Any(p => p.Equals(productName, StringComparison.OrdinalIgnoreCase)))?.CategoryName;
            if (string.IsNullOrEmpty(category))
            {
                return "Other";
            }
            return category;
        }
    }
}
