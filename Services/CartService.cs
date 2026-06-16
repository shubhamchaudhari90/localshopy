using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class CartService : ICartService
    {
        private readonly AppDBContext _context;

        public CartService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<bool> AddProductToCart(string productName, string shopName, string emailId)
        {
            ProductMaster? productMaster = await _context.ProductMasters.AsNoTracking().FirstOrDefaultAsync(pm => pm.ProductName == productName);
            if (productMaster != null)
            {
                Shop? shop = await _context.Shops.FirstOrDefaultAsync(s => s.Name == shopName);
                if (shop != null)
                {
                    Product? product = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.ProductMasterId == productMaster.Id && p.ShopId == shop.Id);

                    if (product != null)
                    {
                        bool isExists = await _context.Carts.AnyAsync(c => c.ProductId == product.Id && c.EmailId == emailId);

                        if (!isExists)
                        {
                            Cart cart = new Cart() { EmailId = emailId, ProductId = product.Id, Quantity = 1 };
                            await _context.Carts.AddAsync(cart);
                            int rows = await _context.SaveChangesAsync();
                            bool result = rows > 0 ? true : false;
                            return result;
                        }
                    }
                }
            }
            return false;
        }

        public async Task<bool> RemoveProductFromCart(Guid productId, string emailId)
        {
            var existingCart = await _context.Carts
                .FirstOrDefaultAsync(c => c.ProductId == productId && c.EmailId == emailId);

            if (existingCart == null)
                return false;

            _context.Carts.Remove(existingCart);
            var rows = await _context.SaveChangesAsync();
            return rows > 0;
        }

        public async Task<List<CartViewModel>> GetCartDetails(string emailId, Guid locationId)
        {
            List<CartViewModel> products = new List<CartViewModel>();

            products = await
                (
                from cart in _context.Carts.AsNoTracking()
                join p in _context.Products.AsNoTracking()
                on cart.ProductId equals p.Id

                join pm in _context.ProductMasters.AsNoTracking()
                on p.ProductMasterId equals pm.Id

                join shop in _context.Shops.AsNoTracking()
                on p.ShopId equals shop.Id

                join cat in _context.Categoties.AsNoTracking()
                on pm.CategoryId equals cat.Id into categoryGroup
                from c in categoryGroup.DefaultIfEmpty() // LEFT JOIN

                where cart.EmailId == emailId
                && p.IsActive
                && p.IsAvailable
                && shop.IsOpen
                && shop.AccountValidTill.Date >= DateTime.Today
                && (c == null || c.IsActive) // ✅ preserve LEFT JOIN
                && shop.ServedLocations.Contains(locationId)

                orderby shop.Name, cart.CreatedAt

                select new CartViewModel
                {
                    ShopId = shop.Id,
                    ProductId = p.Id,
                    ProductMasterId = pm.Id,
                    CategoryId = c != null ? c.Id : Guid.Empty,
                    ShopName = shop.Name,
                    ProductName = pm.ProductName,
                    PackSize = p.PackSize,
                    Unit = p.Unit,
                    ImageFileName = p.ImageFileName,
                    Quantity = cart.Quantity == 0 ? 1 : cart.Quantity,
                    Price = p.Price,
                    Discount = p.Discount,
                    DiscountValidFrom = p.DiscountValidFrom,
                    DiscountValidTill = p.DiscountValidTill,
                    CategoryName = c != null ? c.Name : "Other",
                    Type = p.Type
                }).GroupBy(x => new
                {
                    x.ShopId,
                    x.ProductId,
                    x.ProductMasterId,
                    x.CategoryId,
                    x.ShopName,
                    x.ProductName,
                    x.ImageFileName,
                    x.PackSize,
                    x.Unit,
                    x.Price,
                    x.Discount,
                    x.DiscountValidFrom,
                    x.DiscountValidTill,
                    x.CategoryName,
                    x.Type
                }).Select(g => new CartViewModel
                {
                    ShopId = g.Key.ShopId,
                    ProductId = g.Key.ProductId,
                    ProductMasterId = g.Key.ProductMasterId,
                    CategoryId = g.Key.CategoryId,
                    ShopName = g.Key.ShopName,
                    ProductName = g.Key.ProductName,
                    ImageFileName = g.Key.ImageFileName,
                    PackSize = g.Key.PackSize,
                    Unit = g.Key.Unit,
                    Quantity = g.Max(x => x.Quantity),
                    Price = g.Key.Price,
                    Discount = g.Key.Discount,
                    DiscountValidFrom = g.Key.DiscountValidFrom,
                    DiscountValidTill = g.Key.DiscountValidTill,
                    CategoryName = g.Key.CategoryName,
                    Type = g.Key.Type
                }).ToListAsync();
            return products;
        }

        public async Task<bool> UpdateCartQuantity(Guid productId, int quantity, string emailId)
        {
            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.EmailId == emailId);

            if (cartItem == null)
                return false;

            cartItem.Quantity = quantity;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> GetSocietyName(Guid locationId)
        {
            Location? location = await _context.Locations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == locationId);
            if (location == null) return string.Empty;
            return location.Name;
        }
    }
}
