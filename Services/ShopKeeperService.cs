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
