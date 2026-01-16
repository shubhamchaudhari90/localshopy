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
            ProductMaster? productMaster = await _context.ProductMasters.FirstOrDefaultAsync(pm => pm.ProductName == productName);
            if (productMaster != null)
            {
                Shop? shop = await _context.Shops.FirstOrDefaultAsync(s => s.Name == shopName);
                if (shop != null)
                {
                    Product? product = await _context.Products.FirstOrDefaultAsync(p => p.ProductMasterId == productMaster.Id && p.ShopId == shop.Id);

                    if (product != null)
                    {
                        bool isExists = await _context.Carts.AnyAsync(c => c.ProductId == product.Id && c.EmailId == emailId);

                        if (!isExists)
                        {
                            Cart cart = new Cart() { EmailId = emailId, ProductId = product.Id, Quantity = 1 };
                            await _context.Carts.AddAsync(cart);
                            int rows = _context.SaveChanges();
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
            Cart? existingCart = await _context?.Carts?.FirstOrDefaultAsync(c => c.ProductId == productId && c.EmailId == emailId);

            if (existingCart != null)
            {
                _context.Carts.Remove(existingCart);
                int rows = _context.SaveChanges();
                bool result = rows > 0 ? true : false;
                return result;
            }
            return false;
        }

        public async Task<List<CartViewModel>> GetCartDetails(string emailId, Guid locationId)
        {
            List<CartViewModel> products = new List<CartViewModel>();

            var productIds = await _context.Carts
                .Where(x => x.EmailId == emailId)
                .Select(x => x.ProductId)
                .ToListAsync();

            products = await (
            from p in _context.Products
            join pm in _context.ProductMasters
            on p.ProductMasterId equals pm.Id

            join shop in _context.Shops
            on p.ShopId equals shop.Id

            join cart in _context.Carts
            on p.Id equals cart.ProductId

            join c in _context.Categoties
                on pm.CategoryId equals c.Id into cat
            from c in cat.DefaultIfEmpty() // LEFT JOIN
            where p.IsActive    // NOT DELETED
                && p.IsAvailable    // AVAILABLE ONLY (NO OUT OF STOCK)
                && shop.IsOpen      // SHOP SHOULD BE OPEN
                && shop.AccountValidTill.Date >= DateTime.Today // SHOP ACCOUNT SHOULD BE VALID
                && c.IsActive
                && productIds.Contains(p.Id)
                && shop.ServedLocations.Contains(locationId)

            orderby shop.Name, cart.CreatedAt

            select new CartViewModel
            {
                ShopId = shop.Id,
                ProductId = p.Id,
                ProductMasterId = pm.Id,
                CategoryId = c.Id,
                ShopName = shop.Name,
                ProductName = pm.ProductName,
                ImageFileName = p.ImageFileName,
                Quantity = cart.Quantity == 0 ? 1 : cart.Quantity,
                Price = p.Price,
                Discount = p.Discount,
                DiscountValidFrom = p.DiscountValidFrom,
                DiscountValidTill = p.DiscountValidTill,
                CategoryName = c.Name,
                Type = p.Type

            }).Distinct().ToListAsync();

            return products;
        }

        public async Task<bool> UpdateCartQuantity(Guid productId, int quantity, string emailId)
        {
            var cartItem = await _context.Carts
                .FirstOrDefaultAsync(x => x.ProductId == productId && x.EmailId == emailId);

            if (cartItem == null)
                return false;

            cartItem.Quantity = quantity;
            _context.SaveChanges();

            return true;
        }

    }
}
