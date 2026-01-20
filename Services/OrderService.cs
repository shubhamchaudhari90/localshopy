using localshopyNew.Constants;
using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDBContext _context;

        public OrderService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> PlaceOrder(string emailId, Guid locationId, string flatNumber, string wing)
        {
            var location = await _context.Locations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == locationId);

            if (location == null)
                return new();

            var products = await (
                from cart in _context.Carts
                join p in _context.Products on cart.ProductId equals p.Id
                join pm in _context.ProductMasters on p.ProductMasterId equals pm.Id
                join shop in _context.Shops on p.ShopId equals shop.Id
                join c in _context.Categoties on pm.CategoryId equals c.Id
                where cart.EmailId == emailId
                      && p.IsActive
                      && p.IsAvailable
                      && shop.IsOpen
                      && shop.AccountValidTill.Date >= DateTime.Today
                      && c.IsActive
                      && shop.ServedLocations.Contains(locationId)
                select new CartViewModel
                {
                    ShopId = shop.Id,
                    ShopName = shop.Name,
                    ShopNumber = shop.ShopNumber,
                    ProductId = p.Id,
                    ProductName = pm.ProductName,
                    ProductMasterId = pm.Id,
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    ImageFileName = p.ImageFileName,
                    Quantity = cart.Quantity == 0 ? 1 : cart.Quantity,
                    Price = p.Price,
                    Discount = p.Discount,
                    DiscountValidFrom = p.DiscountValidFrom,
                    DiscountValidTill = p.DiscountValidTill,
                    Type = p.Type
                }
            )
            .AsNoTracking()
            .ToListAsync();

            if (!products.Any())
                return new();

            var today = DateTime.Today;
            var now = DateTime.Now;

            var orderCountByShop = await _context.Orders
                .Where(x => x.CreatedAt.Date == today)
                .GroupBy(x => x.ShopName)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            var orders = new List<Order>();
            var orderTracking = new List<OrderTracking>();
            var orderItems = new List<OrderItem>();

            foreach (var shopGroup in products.GroupBy(x => new { x.ShopId, x.ShopName, x.ShopNumber }))
            {
                var count = orderCountByShop.GetValueOrDefault(shopGroup.Key.ShopName, 0) + 1;

                // Calculate subtotal FIRST
                int subTotal = shopGroup.Sum(p => p.FinalPrice * p.Quantity);

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    EmailId = emailId,
                    ShopId = shopGroup.Key.ShopId,
                    ShopName = shopGroup.Key.ShopName,
                    CreatedAt = now,
                    UpdatedAt = now,
                    OrderNumber = $"{now:yyyy-MM-dd}-{shopGroup.Key.ShopNumber.ToString("D3")}-{count}",

                    Status = OrderStatus.ORDER_PLACED,

                    BillingAddress = $"Flat Number: {flatNumber}, Wing: {wing}, Society Name: {location.Name}, Address: {location.Address}",
                    ShippingAddress = $"Flat Number: {flatNumber}, Wing: {wing}, Society Name: {location.Name}, Address: {location.Address}",

                    PaymentMethod = "CASH/UPI",

                    // 👇 Subtotal & Total
                    Subtotal = subTotal,
                    Tax = 0,                  // set if applicable
                    ShippingFee = 0,
                    TotalAmount = subTotal    // or subTotal + Tax + ShippingFee
                };

                orderTracking.Add(new OrderTracking
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    Status = OrderStatus.ORDER_PLACED,
                    CreatedAt = DateTime.UtcNow
                });

                orders.Add(order);

                orderItems.AddRange(
                    shopGroup.Select(p => new OrderItem
                    {
                        Id = Guid.NewGuid(),
                        OrderId = order.Id,
                        ProductId = p.ProductId,
                        ProductName = p.ProductName,
                        Quantity = p.Quantity,
                        UnitPrice = p.FinalPrice,
                        TotalPrice = p.FinalPrice * p.Quantity,
                        Type = p.Type,
                        ImageFileName = p.ImageFileName,
                    })
                );
            }

            await _context.orderTrackings.AddRangeAsync(orderTracking);
            await _context.Orders.AddRangeAsync(orders);
            await _context.OrderItems.AddRangeAsync(orderItems);

            var cartsToRemove = await _context.Carts
                .Where(x => x.EmailId == emailId)
                .ToListAsync();

            _context.Carts.RemoveRange(cartsToRemove);

            await _context.SaveChangesAsync();

            return await _context.Orders
                .Where(x => orders.Select(o => o.Id).Contains(x.Id))
                .Include(x => x.OrderItems)
                .ToListAsync();
        }

        public async Task<List<Order>> GetAllOrders(string emailId, Guid locationId)
        {
            // Get location first
            var location = await _context.Locations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == locationId);

            if (location == null)
                return new List<Order>();

            // Fetch orders
            var orders = await _context.Orders.AsNoTracking()
                .Where(o => o.EmailId == emailId &&
                            EF.Functions.Like(o.ShippingAddress, $"%{location.Name}%"))
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders;
        }

        public async Task<Order?> GetOrder(Guid id)
        {
            Order? order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);
            return order;
        }

        public async Task<bool> Cancel(Guid id)
        {
            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (order == null || order.Status != OrderStatus.ORDER_PLACED)
                return false;

            order.Status = OrderStatus.CANCELLED;
            int rowsUpdated = _context.SaveChanges();
            return rowsUpdated > 0;
        }
    }
}
