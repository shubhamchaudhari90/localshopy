using localshopyNew.Data;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class ShopProductService : IShopProductService
    {
        private readonly AppDBContext _context;

        public ShopProductService(AppDBContext context)
        {
            _context = context;
        }
        public async Task<List<ProductViewModel>?> GetAllShopProduct(Guid shopId)
        {

            List<ProductViewModel>? products = await (
                from p in _context.Products

                join pm in _context.ProductMasters
                on p.ProductMasterId equals pm.Id

                join c in _context.Categoties
                on pm.CategoryId equals c.Id into cat
                from c in cat.DefaultIfEmpty()   // LEFT JOIN

                join s in _context.Shops
                on p.ShopId equals s.Id

                where s.Id == shopId              // case-insensitive by DB collation
                && p.IsActive                     // product active
                && s.IsActive                     // shop active
                && s.AccountValidTill >= DateTime.Today
                && pm.IsActive
                && (c == null || c.IsActive)      // SAFE left join condition

                orderby p.SortOrder

                select new ProductViewModel
                {
                    ShopName = s.Name,
                    Id = p.Id,
                    ProductMasterId = pm.Id,
                    ShopId = p.ShopId,
                    SortOrder = p.SortOrder,
                    Description = p.Description,
                    Price = p.Price,
                    IsAvailable = p.IsAvailable,
                    ImageFileName = p.ImageFileName,
                    PackSize = p.PackSize,
                    Unit = p.Unit,
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
            return products;
        }

        public async Task<ProductViewModel?> GetProductDetails(Guid productId)
        {

            ProductViewModel? product = await (
                from p in _context.Products

                join pm in _context.ProductMasters
                on p.ProductMasterId equals pm.Id

                join c in _context.Categoties
                on pm.CategoryId equals c.Id into cat
                from c in cat.DefaultIfEmpty()   // LEFT JOIN

                join s in _context.Shops
                on p.ShopId equals s.Id

                where s.Id == p.ShopId              // case-insensitive by DB collation
                && p.Id == productId
                && p.IsActive                     // product active
                && s.IsActive                     // shop active
                && s.AccountValidTill >= DateTime.Today
                && pm.IsActive
                && (c == null || c.IsActive)      // SAFE left join condition

                orderby p.SortOrder

                select new ProductViewModel
                {
                    ShopName = s.Name,
                    Id = p.Id,
                    ProductMasterId = pm.Id,
                    ShopId = p.ShopId,
                    SortOrder = p.SortOrder,
                    Description = p.Description,
                    Price = p.Price,
                    IsAvailable = p.IsAvailable,
                    ImageFileName = p.ImageFileName,
                    PackSize = p.PackSize,
                    Unit = p.Unit,
                    Discount = p.Discount,
                    DiscountValidFrom = p.DiscountValidFrom,
                    DiscountValidTill = p.DiscountValidTill,
                    CreatedAt = p.CreatedAt,
                    IsActive = p.IsActive,
                    CategoryId = c != null ? c.Id : Guid.Empty,
                    ProductMasterName = pm.ProductName,
                    CategoryName = c != null ? c.Name : "Other",
                    Type = string.IsNullOrEmpty(p.Type) ? "VEG" : p.Type
                }).FirstOrDefaultAsync();
            return product;
        }

        public async Task UpdateProduct(ProductViewModel product)
        {
            if (product == null) { return; }
            var existing = await _context.Products.FirstOrDefaultAsync(x => x.Id == product.Id);

            if (existing == null) { return; }

            existing.Description = product.Description;
            existing.Price = product.Price;
            existing.IsAvailable = product.IsAvailable;
            existing.IsActive = product.IsActive;
            existing.Discount = product.Discount;
            existing.DiscountValidFrom = product.DiscountValidFrom;
            existing.DiscountValidTill = product.DiscountValidTill;
            existing.Type = product.Type;
            existing.UpdatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata")); ;

            await _context.SaveChangesAsync();
        }
    }
}
