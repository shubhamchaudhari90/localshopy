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

        public bool IsShopExists(string name, string ownerEmailId)
        {
            var path = GetShopPath(name);

            if (File.Exists(path))
                return true;

            var shops = new List<Shop>();
            var files = Directory.GetFiles(_shopFolder, "*.json");

            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var shop = JsonSerializer.Deserialize<Shop>(json);
                if (shop != null && shop.OwnerEmailId == ownerEmailId)
                    return true;
            }
            return false;
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

        public Shop? GetShopById(string shopId)
        {
            var files = Directory.GetFiles(_shopFolder, "*.json");
            foreach (var file in files)
            {
                var json = File.ReadAllText(file);
                var shop = JsonSerializer.Deserialize<Shop>(json);
                if (shop != null && shop.Id == shopId)
                    return shop;
            }
            return null;
        }

        public void CreateShop(Shop shop)
        {
            var path = GetShopPath(shop.Name);

            if (IsShopExists(shop.Name, shop.OwnerEmailId))
                throw new InvalidOperationException("Shop Name/Owner Email already exists.");

            shop.CreatedAt = DateTime.Now;

            var json = JsonSerializer.Serialize(shop, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(path, json);
        }

        public void UpdateShop(Shop shop)
        {
            if (string.IsNullOrEmpty(shop.Id))
                throw new FileNotFoundException("Shop not found.");

            var existing = GetShopById(shop.Id);
            if (existing == null)
                throw new FileNotFoundException("Shop not found.");

            shop.Products = existing.Products;
            shop.CreatedAt = existing.CreatedAt;
            shop.OwnerEmailId = existing.OwnerEmailId;

            if (shop.Name != existing.Name)
            {
                CreateShop(shop);
                DeleteShop(existing.Name);
            }
            else
            {
                var json = JsonSerializer.Serialize(shop, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                var path = GetShopPath(existing.Name);
                File.WriteAllText(path, json);
            }
        }

        public void DeleteShop(string shopName)
        {
            var path = GetShopPath(shopName);

            if (!File.Exists(path))
                throw new FileNotFoundException("Shop does not exist.");

            File.Delete(path);
        }
    }
}
