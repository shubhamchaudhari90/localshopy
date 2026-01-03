using localshopyNew.Data;
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

        public async Task<List<ProductViewModel>> GetProductsByLocation(Guid id)
        {
            var products = await (
                from p in _context.Products
                join pm in _context.ProductMasters
                on p.ProductMasterId equals pm.Id

                join shop in _context.Shops
                on p.ShopId equals shop.Id

                join c in _context.Categoties
                on pm.CategoryId equals c.Id into cat
                from c in cat.DefaultIfEmpty() // LEFT JOIN

                where p.IsActive
                && shop.ServedLocations.Contains(id)

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
                    CategoryName = c != null ? c.Name : "Other"
                }).ToListAsync();

            return products;
        }
    }
}
