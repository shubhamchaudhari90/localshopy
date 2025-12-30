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

            return new ShopProductsViewModel
            {
                Shop = shop,
                Products = productViewModels
            };
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
