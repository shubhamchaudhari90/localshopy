using localshopyNew.Models;
using System.Text.Json;

namespace localshopyNew.Services
{
    public class ShopService
    {
        private readonly string _shopFolder;

        public ShopService(IWebHostEnvironment env)
        {
            _shopFolder = Path.Combine(env.ContentRootPath, "App_Data", "Shops");
            Directory.CreateDirectory(_shopFolder);
        }

        private string GetShopPath(string Name)
            => Path.Combine(_shopFolder, $"{Name}.json");

        public Shop GetShop(string Name)
        {
            var path = GetShopPath(Name);

            if (!File.Exists(path))
                throw new FileNotFoundException("Shop not found.");

            var json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<Shop>(json)!;
        }

        public List<Shop> GetAllShops()
        {
            var shops = new List<Shop>();
            var files = Directory.GetFiles(_shopFolder, "*.json");

            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var shop = JsonSerializer.Deserialize<Shop>(json);
                if (shop != null)
                    shops.Add(shop);
            }

            return shops;
        }

        public void CreateShop(Shop shop)
        {
            var path = GetShopPath(shop.Name);

            if (File.Exists(path))
                throw new InvalidOperationException("Shop already exists.");

            var json = JsonSerializer.Serialize(shop, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }

        public void UpdateShop(Shop shop)
        {
            var path = GetShopPath(shop.Name);

            if (!File.Exists(path))
                throw new FileNotFoundException("Shop not found.");

            Shop existing = GetShop(shop.Name);
            shop.Products = existing.Products;
            shop.Id = existing.Id;

            var json = JsonSerializer.Serialize(shop, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }
    }
}
