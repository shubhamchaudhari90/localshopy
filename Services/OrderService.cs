using localshopyNew.Constants;
using localshopyNew.Data;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace localshopyNew.Services
{
    public class OrderService : IOrderService
    {
        private readonly AppDBContext _context;

        public OrderService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<Order>> GetAllOrders(string emailId, Guid locationId)
        {
            // Get location first
            var location = await _context.Locations.AsNoTracking().FirstOrDefaultAsync(x => x.Id == locationId);

            if (location == null)
                return new List<Order>();

            // Fetch orders
            var last50Days = DateTime.Now.Date.AddDays(-50);

            var orders = await _context.Orders.AsNoTracking()
                .Where(o => o.EmailId == emailId && o.CreatedAt >= last50Days &&
                            EF.Functions.Like(o.ShippingAddress, $"%{location.Name}%"))
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders;
        }

        public async Task<Order?> GetOrderById(Guid id)
        {
            Order? order = await _context.Orders.Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);
            return order;
        }

        public async Task<List<Order>> GetAllOrdersByShopId(Guid shopId)
        {
            var last50Days = DateTime.Now.Date.AddDays(-50);

            List<Order> orders = await _context.Orders.Where(x => x.ShopId == shopId && x.CreatedAt >= last50Days)
                .AsNoTracking().Include(x => x.OrderItems).OrderBy(x => x.CreatedAt).ToListAsync();
            return orders;
        }

        public async Task<List<Order>> OrdersToServe(Guid shopId)
        {
            var last50Days = DateTime.Now.Date.AddDays(-50);

            List<Order> orders = await _context.Orders.Where(x => x.ShopId == shopId &&
            (x.Status == OrderStatus.ORDER_PLACED || x.Status == OrderStatus.ACCEPTED
            || x.Status == OrderStatus.PROCESSING || x.Status == OrderStatus.OUT_FOR_DELIVERY
            ))
                .AsNoTracking().Include(x => x.OrderItems).OrderBy(x => x.CreatedAt).ToListAsync();
            return orders;
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
                    OrderNumber = $"{now:yyyyMMdd}-{shopGroup.Key.ShopNumber.ToString("D2")}-{count}",

                    Status = OrderStatus.ORDER_PLACED,

                    BillingAddress = $"Flat no.: {flatNumber}, Wing: {wing}, Society: {location.Name}, {location.Address}",
                    ShippingAddress = $"Flat no.: {flatNumber}, Wing: {wing}, Society: {location.Name}, {location.Address}",

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

        public async Task<bool> Cancel(Guid id)
        {
            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (order == null || order.Status != OrderStatus.ORDER_PLACED)
                return false;

            order.Status = OrderStatus.CANCELLED;
            order.UpdatedAt = DateTime.Now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.CANCELLED,
                CreatedAt = DateTime.Now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = _context.SaveChanges();
            return rowsUpdated > 0;
        }

        public async Task<bool> Accept(Guid id, Guid shopId)
        {
            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null || order.Status != OrderStatus.ORDER_PLACED)
                return false;

            order.Status = OrderStatus.ACCEPTED;
            order.UpdatedAt = DateTime.Now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.ACCEPTED,
                CreatedAt = DateTime.Now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = _context.SaveChanges();
            return rowsUpdated > 0;
        }

        public async Task<bool> Reject(Guid id, Guid shopId)
        {
            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null || order.Status != OrderStatus.ORDER_PLACED)
                return false;

            order.Status = OrderStatus.REJECTED;
            order.UpdatedAt = DateTime.Now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.REJECTED,
                CreatedAt = DateTime.Now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = _context.SaveChanges();
            return rowsUpdated > 0;
        }

        public async Task<bool> Processing(Guid id, Guid shopId)
        {
            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null || order.Status != OrderStatus.ACCEPTED)
                return false;

            order.Status = OrderStatus.PROCESSING;
            order.UpdatedAt = DateTime.Now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.PROCESSING,
                CreatedAt = DateTime.Now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = _context.SaveChanges();
            return rowsUpdated > 0;
        }

        public async Task<bool> OutForDelivery(Guid id, Guid shopId)
        {
            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null || order.Status != OrderStatus.PROCESSING)
                return false;

            order.Status = OrderStatus.OUT_FOR_DELIVERY;
            order.UpdatedAt = DateTime.Now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.OUT_FOR_DELIVERY,
                CreatedAt = DateTime.Now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = _context.SaveChanges();
            return rowsUpdated > 0;
        }

        public async Task<bool> Delivered(Guid id, Guid shopId)
        {
            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null || order.Status != OrderStatus.OUT_FOR_DELIVERY)
                return false;

            order.Status = OrderStatus.DELIVERED;
            order.UpdatedAt = DateTime.Now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.DELIVERED,
                CreatedAt = DateTime.Now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = _context.SaveChanges();
            return rowsUpdated > 0;
        }

        public async Task<bool> PreOrder(Guid id, Guid shopId)
        {
            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null)
                return false;

            order.UpdatedAt = DateTime.Now;
            order.IsPreOrder = true;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.PREORDER,
                CreatedAt = DateTime.Now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = _context.SaveChanges();
            return rowsUpdated > 0;
        }

        public async Task<bool> SaveToken(string emailId, string token, string role)
        {

            var alldevices = await _context.UserDevices.ToListAsync();

            var existing = await _context.UserDevices
                .FirstOrDefaultAsync(x => x.EmailId == emailId && x.FcmToken == token);

            if (existing == null && !string.IsNullOrEmpty(emailId) && role != null && !string.IsNullOrEmpty(token))
            {
                _context.UserDevices.Add(new UserDevice
                {
                    EmailId = emailId,
                    Role = role,
                    FcmToken = token
                });
                int rowsUpdated = 0;
                try
                {
                    rowsUpdated = _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    string msg = ex.Message;
                }
                return rowsUpdated > 0;
            }
            return false;
        }

        public async Task<List<string>> GetToken(Guid orderID)
        {
            var order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderID);

            var alldevices = await _context.UserDevices.ToListAsync();
            var tokens = await _context.UserDevices
                .Where(x => x.EmailId == order.EmailId)
                .Select(x => x.FcmToken)
                .ToListAsync();

            return tokens;
        }

        public async Task<List<string>> GetShopkeeperTokens(List<Guid> orderIds)
        {
            List<Guid> shopIds = await _context.Orders.Where(x => orderIds.Contains(x.Id)).Select(x => x.ShopId).ToListAsync();

            if (shopIds.Count == 0)
                return [];

            List<string> emailIds = await _context.Shops.Where(x => shopIds.Contains(x.Id)).Select(x => x.OwnerEmailId).ToListAsync();

            var tokens = await _context.UserDevices.Where(x => emailIds.Contains(x.EmailId)).Select(x => x.FcmToken).ToListAsync();

            return tokens;
        }
    }
}
