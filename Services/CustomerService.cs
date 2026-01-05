using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDBContext _context;

        public CustomerService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<ProductViewModel>> GetProductsByCategories(string categories)
        {
            List<ProductViewModel> products = new List<ProductViewModel>();

            if (string.IsNullOrEmpty(categories))
            {
                return products;
            }
            List<string> categoryList = categories.Split(", ").ToList();

            products = await (
                from p in _context.Products
                join pm in _context.ProductMasters
                on p.ProductMasterId equals pm.Id

                join shop in _context.Shops
                on p.ShopId equals shop.Id

                join c in _context.Categoties
                on pm.CategoryId equals c.Id into cat
                from c in cat.DefaultIfEmpty() // LEFT JOIN

                where p.IsActive    // NOT DELETED
                && p.IsAvailable    // AVAILABLE ONLY (NO OUT OF STOCK)
                && shop.IsOpen      // SHOP SHOULD BE OPEN
                && shop.AccountValidTill.Date >= DateTime.Today // SHOP ACCOUNT SHOULD BE VALID
                && c.IsActive
                && categoryList.Contains(c.Name)

                orderby p.CreatedAt

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
                    CategoryId = c != null ? c.Id : Guid.Empty,
                    ProductMasterName = pm.ProductName,
                    CategoryName = c != null ? c.Name : "Other",
                    ShopName = shop.Name,
                    Type = p.Type
                }).ToListAsync();

            return products;
        }

        public async Task<List<Categoty>> GetCategoriesByLocation(Guid locationId)
        {
            var categories = await (
                from p in _context.Products
                join pm in _context.ProductMasters
                on p.ProductMasterId equals pm.Id

                join shop in _context.Shops
                on p.ShopId equals shop.Id

                join c in _context.Categoties
                on pm.CategoryId equals c.Id into cat
                from c in cat.DefaultIfEmpty() // LEFT JOIN

                where p.IsActive    // NOT DELETED
                && p.IsAvailable    // AVAILABLE ONLY (NO OUT OF STOCK)
                && shop.IsOpen      // SHOP SHOULD BE OPEN
                && shop.AccountValidTill.Date >= DateTime.Today // SHOP ACCOUNT SHOULD BE VALID
                && shop.ServedLocations.Contains(locationId) // SERVED LOCATION BY SHOP
                && c.IsActive

                //orderby c.Name

                select new Categoty
                {
                    Name = c.Name,
                    SortOrder = c.SortOrder
                }).Distinct().ToListAsync();

            return categories;
        }

        public async Task<ShopProductsViewModel?> GetShopDetailsByName(string shopName)
        {
            var products = await (
                from p in _context.Products

                join pm in _context.ProductMasters
                on p.ProductMasterId equals pm.Id

                join c in _context.Categoties
                on pm.CategoryId equals c.Id into cat
                from c in cat.DefaultIfEmpty()   // LEFT JOIN

                join s in _context.Shops
                on p.ShopId equals s.Id

                where s.Name == shopName              // case-insensitive by DB collation
                && p.IsActive                     // product active
                && s.IsActive                     // shop active
                && s.AccountValidTill >= DateTime.Today
                && pm.IsActive
                && (c == null || c.IsActive)      // SAFE left join condition

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
                    CategoryId = c != null ? c.Id : Guid.Empty,
                    ProductMasterName = pm.ProductName,
                    CategoryName = c != null ? c.Name : "Other",
                    Type = string.IsNullOrEmpty(p.Type) ? "VEG" : p.Type
                }).ToListAsync();


            var shop = await _context.Shops.FirstOrDefaultAsync(x => x.Name.ToLower() == shopName.ToLower());

            List<string> locations = new List<string>();

            if (shop != null)
            {
                locations = await _context.Locations.Where(x => shop.ServedLocations.Contains(x.Id)).Select(x => x.Name).ToListAsync();
            }


            return new ShopProductsViewModel
            {
                Shop = shop,
                Locations = locations,
                Products = products
            };
        }
    }
}
