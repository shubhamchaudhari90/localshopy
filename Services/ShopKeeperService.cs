namespace localshopyNew.Services
{
    public class ShopkeeperService
    {
        //private readonly string _shopFolder;
        //private readonly string _productsFilePath;
        //public ShopkeeperService(IWebHostEnvironment env)
        //{
        //    _shopFolder = Path.Combine(env.ContentRootPath, "App_Data", "Shops");
        //    _productsFilePath = Path.Combine(env.ContentRootPath, "App_Data", "Products.json");
        //    Directory.CreateDirectory(_shopFolder);
        //}

        //public Shop? IsShopExists(string email, string password)
        //{
        //    if (!Directory.Exists(_shopFolder))
        //        throw new DirectoryNotFoundException("Shops folder not found.");

        //    var files = Directory.GetFiles(_shopFolder, "*.json");

        //    foreach (var file in files)
        //    {
        //        var json = File.ReadAllText(file);
        //        var shop = JsonSerializer.Deserialize<Shop>(json);

        //        if (shop != null &&
        //            shop.OwnerEmailId.Equals(email, StringComparison.OrdinalIgnoreCase) &&
        //            shop.Password == password)
        //        {
        //            return shop; // Found matching shop
        //        }
        //    }

        //    return null; // No match
        //}


        //public Shop GetShop(string Name)
        //{
        //    var path = GetShopPath(Name);

        //    if (!File.Exists(path))
        //        throw new FileNotFoundException("Shop not found.");

        //    var json = File.ReadAllText(path);
        //    return JsonSerializer.Deserialize<Shop>(json)!;
        //}

        //public Shop? GetShopById(string id)
        //{
        //    var files = Directory.GetFiles(_shopFolder, "*.json");

        //    // 1. Read all JSON files
        //    foreach (var jsonFile in files)
        //    {
        //        var json = File.ReadAllText(jsonFile);

        //        Shop? shop = JsonSerializer.Deserialize<Shop>(json);
        //        if (shop?.Id != id) continue;
        //        return shop;
        //    }
        //    return null;
        //}

        //public Shop GetShopByFile(string filePath)
        //{
        //    if (!File.Exists(filePath))
        //        throw new FileNotFoundException($"Shop file '{filePath}' not found.");

        //    var json = File.ReadAllText(filePath);
        //    return JsonSerializer.Deserialize<Shop>(json) ?? throw new Exception("Invalid shop data.");

        //}

        //private string GetShopPath(string Name)
        //    => Path.Combine(_shopFolder, $"{Name}.json");

        //public void AddProduct(Shop shop, Product product)
        //{
        //    if (shop.Products.Any(p => p.Name == product.Name))
        //        throw new Exception("Product with same name already exists.");

        //    if (!IsProductValid(product.Name))
        //        throw new Exception("Product not found.");

        //    shop.Products.Add(product);
        //    // Save shop file
        //    var fileName = shop.Name + ".json"; // Or keep mapping of file names
        //    SaveShop(fileName, shop);
        //}

        //public void UpdateProduct(Shop shop, Product product)
        //{
        //    var existing = shop.Products.FirstOrDefault(p => p.Name == product.Name);
        //    if (existing == null)
        //        throw new Exception("Product not found.");

        //    if (!IsProductValid(product.Name))
        //        throw new Exception("Product not found.");


        //    if (!string.IsNullOrEmpty(product.ImageFileName))
        //    {
        //        existing.ImageFileName = product.ImageFileName;
        //    }

        //    existing.Description = product.Description;
        //    existing.Price = product.Price;
        //    existing.IsAvailable = product.IsAvailable;
        //    existing.Discount = product.Discount;
        //    existing.DiscountValidFrom = product.DiscountValidFrom;
        //    existing.DiscountValidTill = product.DiscountValidTill;

        //    var fileName = shop.Name + ".json"; // Or keep mapping of file names
        //    SaveShop(fileName, shop);
        //    RemoveUnusedImages();
        //}

        //public void UpdateFromShopkeeper(Shop shop)
        //{
        //    var path = GetShopPath(shop.Name);

        //    if (!File.Exists(path))
        //        throw new FileNotFoundException("Shop not found.");

        //    Shop existing = GetShop(shop.Name);
        //    shop.Id = existing.Id;
        //    shop.Name = existing.Name;
        //    shop.OwnerEmailId = existing.OwnerEmailId;
        //    shop.PhoneNo = existing.PhoneNo;
        //    shop.Products = existing.Products;

        //    var json = JsonSerializer.Serialize(shop, new JsonSerializerOptions
        //    {
        //        WriteIndented = true
        //    });

        //    File.WriteAllText(path, json);
        //}


        //public void DeleteProduct(Shop shop, string productName)
        //{
        //    var product = shop.Products.FirstOrDefault(p => p.Name == productName);
        //    if (product == null)
        //        throw new Exception("Product not found.");

        //    if (!IsProductValid(product.Name))
        //        throw new Exception("Product not found.");

        //    shop.Products.Remove(product);
        //    var fileName = shop.Name + ".json"; // Or keep mapping of file names
        //    SaveShop(fileName, shop);
        //    RemoveUnusedImages();
        //}

        //private void SaveShop(string fileName, Shop shop)
        //{
        //    var filePath = Path.Combine(_shopFolder, fileName);
        //    var json = JsonSerializer.Serialize(shop, new JsonSerializerOptions { WriteIndented = true });
        //    File.WriteAllText(filePath, json);
        //}

        //private void RemoveUnusedImages()
        //{
        //    var usedImages = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        //    if (!Directory.Exists(_shopFolder))
        //        throw new DirectoryNotFoundException("Shops folder not found.");

        //    var files = Directory.GetFiles(_shopFolder, "*.json");

        //    // 1. Read all JSON files
        //    foreach (var jsonFile in files)
        //    {
        //        var json = File.ReadAllText(jsonFile);

        //        Shop? shop = JsonSerializer.Deserialize<Shop>(json);
        //        if (shop?.Products == null) continue;

        //        foreach (var product in shop.Products)
        //        {
        //            if (!string.IsNullOrWhiteSpace(product.ImageFileName))
        //            {
        //                usedImages.Add(product.ImageFileName);
        //            }
        //        }
        //    }

        //    // 2. Get all images from folder
        //    var allImages = Directory.GetFiles("wwwroot/images/products");

        //    // 3. Delete unused images
        //    foreach (var imagePath in allImages)
        //    {
        //        var imageName = Path.GetFileName(imagePath);

        //        if (!usedImages.Contains(imageName))
        //        {
        //            File.Delete(imagePath);
        //        }
        //    }
        //}

        //private bool IsProductValid(string productName)
        //{
        //    if (!File.Exists(_productsFilePath))
        //        return false;

        //    var json = File.ReadAllText(_productsFilePath);
        //    var categories = JsonSerializer.Deserialize<List<ProductMaster>>(json) ?? new();

        //    var products = categories
        //        .SelectMany(c => c.ProductName)
        //        .Distinct(StringComparer.OrdinalIgnoreCase)
        //        .ToList();

        //    return products.Any(x => x.Equals(productName, StringComparison.OrdinalIgnoreCase));
        //}
    }
}
