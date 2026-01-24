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

        public async Task<ShopProductsViewModel?> GetShopDetailsByEmailId(string emailId)
        {
            Shop? shop = await GetShopByEmailId(emailId);

            if (shop == null || shop.Id == Guid.Empty)
            {
                return null;
            }

            ShopProductsViewModel? shopDetails = await GetShopDetailsById(shop.Id);
            return shopDetails;
        }

        public async Task<Shop?> GetShopByLoginModel(LoginViewModel model)
        {
            if (string.IsNullOrEmpty(model.Email) || string.IsNullOrEmpty(model.Password))
                return null;

            var shop = await _context.Shops.FirstOrDefaultAsync(x => x.OwnerEmailId.ToLower() == model.Email.ToLower() && x.IsActive);
            if (shop == null)
                return null;

            if (shop.Password == model.Password)
                return shop;

            return null;
        }

        private async Task<Shop?> GetShopById(Guid shopId)
        {
            var shop = await _context.Shops.FirstOrDefaultAsync(x => x.Id == shopId);

            if (shop == null)
                return null;

            return shop;
        }

        private async Task<Shop?> GetShopByEmailId(string emailId)
        {
            var shop = await _context.Shops.FirstOrDefaultAsync(x => x.IsActive && x.OwnerEmailId.ToLower() == emailId.ToLower());

            if (shop == null)
                return null;

            return shop;
        }

        public async Task<ShopProductsViewModel?> GetShopDetailsById(Guid id)
        {
            var shop = await GetShopById(id);

            if (shop == null)
                return null;

            var products = await (
                from p in _context.Products
                join pm in _context.ProductMasters
                on p.ProductMasterId equals pm.Id

                join c in _context.Categoties
                on pm.CategoryId equals c.Id into cat
                from c in cat.DefaultIfEmpty() // LEFT JOIN

                where p.ShopId == shop.Id
                && p.IsActive
                && shop.IsActive
                orderby p.SortOrder
                select new ProductViewModel
                {
                    Id = p.Id,
                    ProductMasterId = pm.Id,
                    ShopId = p.ShopId,
                    SortOrder = p.SortOrder,
                    Description = p.Description,
                    Price = p.Price,
                    IsAvailable = p.IsAvailable,
                    ImageFileName = p.ImageFileName,
                    Discount = p.Discount,
                    DiscountValidFrom = p.DiscountValidFrom,
                    DiscountValidTill = p.DiscountValidTill,
                    CreatedAt = p.CreatedAt,
                    IsActive = p.IsActive,
                    CategoryId = c.Id,
                    ProductMasterName = pm.ProductName,
                    CategoryName = c != null ? c.Name : "Other",
                    Type = string.IsNullOrEmpty(p.Type) ? "VEG" : p.Type,
                }).ToListAsync();

            List<string> locations = await _context.Locations.Where(x => shop.ServedLocations.Contains(x.Id)).Select(x => x.Name).ToListAsync();

            return new ShopProductsViewModel
            {
                Shop = shop,
                Locations = locations,
                Products = products
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

            bool productExists = await _context.Products.AnyAsync(x => x.ShopId == product.ShopId
            && x.ProductMasterId == product.ProductMasterId && x.IsActive);
            if (productExists)
                return false;
            return true;
        }

        public async Task<bool> AddProductInShop(Product product)
        {
            Product? existing = await _context.Products.FirstOrDefaultAsync(x => x.ShopId == product.ShopId
            && x.ProductMasterId == product.ProductMasterId);
            if (existing != null)
            {
                int count = _context.Products.Where(x => x.ShopId == product.ShopId).Max(x => x.SortOrder);
                existing.SortOrder = count + 1;
                existing.Description = product.Description;
                existing.Price = product.Price;
                existing.Type = product.Type;
                existing.IsAvailable = product.IsAvailable;
                existing.ImageFileName = product.ImageFileName;
                existing.Discount = product.Discount;
                existing.DiscountValidFrom = product.DiscountValidFrom;
                existing.DiscountValidTill = product.DiscountValidTill;
                existing.UpdatedAt = DateTime.Now;
                existing.IsActive = true;
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                product.Id = Guid.NewGuid();
                product.CreatedAt = DateTime.Now;
                product.UpdatedAt = DateTime.Now;
                product.IsActive = true;
                int count = _context.Products.Any(x => x.ShopId == product.ShopId) ? _context.Products.Where(x => x.ShopId == product.ShopId).Max(x => x.SortOrder) : 0;
                product.SortOrder = count + 1;
                await _context.AddAsync(product);
                int rowsInserted = await _context.SaveChangesAsync();
                if (rowsInserted > 0)
                    return true;
            }
            return false;
        }

        public async Task<string?> UpdateProductInShop(Product product)
        {
            Product? existing = await _context.Products.FirstOrDefaultAsync(x => x.Id == product.Id);
            if (existing != null)
            {
                string? oldImageFileName = existing.ImageFileName;
                existing.Price = product.Price;
                existing.IsAvailable = product.IsAvailable;
                if (!string.IsNullOrEmpty(product.ImageFileName))
                    existing.ImageFileName = product.ImageFileName;
                existing.Discount = product.Discount;
                existing.DiscountValidFrom = product.DiscountValidFrom;
                existing.DiscountValidTill = product.DiscountValidTill;
                existing.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
                return oldImageFileName;
            }
            return null;
        }

        public async Task<bool> SwitchStatus(Guid shopId)
        {
            Shop? shop = await _context.Shops.FirstOrDefaultAsync(x => x.Id == shopId);
            if (shop != null)
            {
                shop.IsOpen = !shop.IsOpen;
                _context.SaveChanges();
                return true;
            }
            return false;
        }

        public async Task<bool> DeleteProductFromShop(Guid shopId, Guid productId)
        {
            Product? existing = await _context.Products.FirstOrDefaultAsync(x => x.ShopId == shopId && x.Id == productId);
            if (existing != null)
            {
                existing.IsActive = false;
                existing.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
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
                IsActive = product.IsActive,
                Type = product.Type
            };
            ProductMaster? productMaster = await _context.ProductMasters.FirstOrDefaultAsync(x => x.Id == product.ProductMasterId);
            if (productMaster == null) return null;
            model.ProductMasterName = productMaster.ProductName;
            model.CategoryId = productMaster.CategoryId;
            Category? category = await _context.Categoties.FirstOrDefaultAsync(x => x.Id == model.CategoryId);
            if (category == null) return null;
            model.CategoryName = category.Name;
            return model;
        }

        public async Task<List<string?>> GetAllImageNames()
        {
            List<string?> names = await _context.Products.Select(x => x.ImageFileName).ToListAsync();
            return names;
        }
    }
}
