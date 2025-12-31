using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class ShopkeeperService : IShopkeeperService
    {
        private readonly AppDBContext _context;

        public ShopkeeperService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<Shop?> GetShopByLoginModel(LoginViewModel model)
        {
            var shop = await _context.Shops.FirstOrDefaultAsync(x => x.OwnerEmailId == model.Email);
            if (shop == null)
                return null;
            if (shop.Password == model.Password)
                return shop;
            return null;
        }

        public async Task<Shop?> GetShopById(Guid shopId)
        {
            var shop = await _context.Shops.FirstOrDefaultAsync(x => x.Id == shopId);
            if (shop == null)
                return null;
            return shop;
        }

        public async Task<ShopProductsViewModel?> GetShopDetailsById(Guid id)
        {
            var shop = await GetShopById(id);
            if (shop == null)
                return null;

            var products = await _context.Products
                .Where(x => x.ShopId == shop.Id)
                .OrderBy(x => x.SortOrder)
                .ToListAsync();

            var productViewModels = MapProducts(products);

            List<string> locations = await _context.Locations.Where(x => shop.ServedLocations.Contains(x.Id)).Select(x => x.Name).ToListAsync();

            return new ShopProductsViewModel
            {
                Shop = shop,
                Locations = locations,
                Products = productViewModels
            };
        }

        public async Task<ShopProductsViewModel?> UpdateShopData(Shop shop)
        {
            var existingShop = await _context.Shops.FindAsync(shop.Id);
            if (existingShop == null)
                return null;
            if (!string.IsNullOrEmpty(shop.Password))
                existingShop.Password = shop.Password;
            existingShop.IsOpen = shop.IsOpen;
            existingShop.ServedLocations = shop.ServedLocations;

            await _context.SaveChangesAsync();
            return await GetShopDetailsById(shop.Id);
        }

        public async Task<bool> IsProductValid(Product product)
        {
            if (product == null || product.Price <= 0 || product.Discount < 0)
            {
                return false;
            }

            bool isPrductMasterValid = await _context.ProductMasters.AnyAsync(x => x.Id == product.ProductMasterId);
            if (!isPrductMasterValid)
            {
                return false;
            }

            bool productExists = await _context.Products.AnyAsync(x => x.ShopId == product.ShopId && x.ProductMasterId == product.ProductMasterId);
            if (productExists)
                return false;
            return true;
        }

        public async Task<bool> AddProductInShop(Product product)
        {
            product.Id = Guid.NewGuid();
            product.CreatedAt = DateTime.Now;
            product.IsActive = true;
            int count = _context.Products.Any(x => x.ShopId == product.ShopId) ? _context.Products.Where(x => x.ShopId == product.ShopId).Max(x => x.SortOrder) : 0;
            product.SortOrder = count + 1;
            await _context.AddAsync(product);
            int rowsInserted = await _context.SaveChangesAsync();
            if (rowsInserted > 0)
            {
                return true;
            }
            return false;
        }

        public async Task<ProductViewModel?> GetProductById(Guid id)
        {
            Product? product = await _context.Products.FirstOrDefaultAsync(x => x.Id == id);
            if (product == null) return null;
            var model = new ProductViewModel
            {
                Id = product.Id,
                ProductMasterId = product.ProductMasterId,
                ShopId = product.ShopId,
                SortOrder = product.SortOrder,
                Description = product.Description,
                Price = product.Price,
                IsAvailable = product.IsAvailable,
                ImageFileName = product.ImageFileName,
                Discount = product.Discount,
                DiscountValidFrom = product.DiscountValidFrom,
                DiscountValidTill = product.DiscountValidTill,
                IsActive = product.IsActive
            };
            ProductMaster? productMaster = await _context.ProductMasters.FirstOrDefaultAsync(x => x.Id == product.ProductMasterId);
            if (productMaster == null) return null;
            model.CategoryId = productMaster.CategoryId;

            return model;
        }

        private List<ProductViewModel> MapProducts(List<Product> products)
        {
            var productList = new List<ProductViewModel>();

            foreach (var product in products)
            {
                var model = new ProductViewModel
                {
                    Id = product.Id,
                    ProductMasterId = product.ProductMasterId,
                    ShopId = product.ShopId,
                    SortOrder = product.SortOrder,
                    Description = product.Description,
                    Price = product.Price,
                    IsAvailable = product.IsAvailable,
                    ImageFileName = product.ImageFileName,
                    Discount = product.Discount,
                    DiscountValidFrom = product.DiscountValidFrom,
                    DiscountValidTill = product.DiscountValidTill,
                    IsActive = product.IsActive
                };
                productList.Add(model);
            }
            return productList;
        }
    }
}
