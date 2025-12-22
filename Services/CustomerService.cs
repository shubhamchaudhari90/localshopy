using localshopyNew.Models;
using System.Text.Json;

namespace localshopyNew.Services
{
    public class CustomerService
    {
        private readonly string _locationsPath;
        private readonly string _shopFolder;

        public CustomerService(IWebHostEnvironment env)
        {
            _shopFolder = Path.Combine(env.ContentRootPath, "App_Data", "Shops");
            _locationsPath = Path.Combine(env.ContentRootPath, "App_Data", "Locations.json");
            Directory.CreateDirectory(_shopFolder);
        }

        public List<CustomerProductViewModel> GetAllProducts(string location)
        {
            var allProducts = new List<CustomerProductViewModel>();

            var files = Directory.GetFiles(_shopFolder, "*.json");

            foreach (var file in files)
            {
                var json = System.IO.File.ReadAllText(file);
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
                            Product = product
                        });
                    }
                }
            }
            return allProducts;
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
    }
}
