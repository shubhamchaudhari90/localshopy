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
        private readonly int _oldOrdersDays = -60;

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

            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone).Date;
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            // Fetch orders
            var last60Days = today.AddDays(-60);

            var orders = await _context.Orders.AsNoTracking()
                .Where(o => o.EmailId == emailId && o.CreatedAt >= last60Days &&
                            EF.Functions.Like(o.ShippingAddress, $"%{location.Name}%"))
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return orders;
        }

        public async Task<Order?> GetOrderById(Guid id)
        {
            return await _context.Orders.AsNoTracking().Include(o => o.OrderItems).FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<List<Order>> GetAllOrdersByShopId(Guid shopId)
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone).Date;

            var last60Days = today.AddDays(-60);

            List<Order> orders = await _context.Orders.Where(x => x.ShopId == shopId && x.CreatedAt >= last60Days)
                .AsNoTracking().Include(x => x.OrderItems).OrderBy(x => x.CreatedAt).ToListAsync();
            return orders;
        }

        public async Task<List<Order>> OrdersToServe(Guid shopId)
        {
            List<Order> orders = await _context.Orders.Where(x => x.ShopId == shopId &&
            (x.Status == OrderStatus.ORDER_PLACED || x.Status == OrderStatus.ACCEPTED
            || x.Status == OrderStatus.PROCESSING || x.Status == OrderStatus.OUT_FOR_DELIVERY
            ))
                .AsNoTracking().Include(x => x.OrderItems).OrderBy(x => x.CreatedAt).ToListAsync();
            return orders;
        }

        public async Task<List<Order>> PlaceOrder(string emailId, Guid locationId, string flatNumber, string wing, string mobileNumber, string societyAddress = "")
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone).Date;
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            var oldOrders = await _context.Orders.Where(o => o.CreatedAt < today.AddDays(_oldOrdersDays)).ToListAsync();

            if (oldOrders.Any())
            {
                _context.Orders.RemoveRange(oldOrders);
            }

            var location = await _context.Locations
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == locationId);

            if (location == null)
                return new();

            var allIndiaLocation = await _context.Locations.FirstOrDefaultAsync(x => x.Name == "All India");
            allIndiaLocation ??= new Location() { Name = "Test", Id = locationId };

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
                      && (shop.ServedLocations.Contains(locationId) || shop.ServedLocations.Contains(allIndiaLocation.Id))
                select new CartViewModel
                {
                    ShopId = shop.Id,
                    ShopName = shop.Name,
                    ShopNumber = shop.ShopNumber,
                    ProductId = p.Id,
                    ProductName = pm.ProductName,
                    ProductDescription = string.IsNullOrEmpty(p.Description) ? "" : p.Description,
                    ProductMasterId = pm.Id,
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    ImageFileName = p.ImageFileName,
                    Quantity = cart.Quantity == 0 ? 1 : cart.Quantity,
                    Price = p.Price,
                    Discount = p.Discount,
                    DiscountValidFrom = p.DiscountValidFrom,
                    DiscountValidTill = p.DiscountValidTill,
                    Type = p.Type,
                    ShopContactNumber = $"{shop.PhoneNo}, {shop.AlternateNumber}",
                    PackSize = p.PackSize,
                    Unit = p.Unit
                }
            )
            .AsNoTracking()
            .ToListAsync();

            if (!products.Any())
                return new();

            var orderCountByShop = await _context.Orders
                .Where(x => x.CreatedAt.Date == today)
                .GroupBy(x => x.ShopName)
                .Select(g => new { g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Key, x => x.Count);

            var orders = new List<Order>();
            var orderTracking = new List<OrderTracking>();
            var orderItems = new List<OrderItem>();

            foreach (var shopGroup in products.GroupBy(x => new { x.ShopId, x.ShopName, x.ShopNumber, x.ShopContactNumber }))
            {
                var society = string.IsNullOrWhiteSpace(societyAddress) ? $"{location.Name}, {location.Address}" : societyAddress;
                var count = orderCountByShop.GetValueOrDefault(shopGroup.Key.ShopName, 0) + 1;

                // Calculate subtotal FIRST
                int subTotal = shopGroup.Sum(p => p.FinalPrice * p.Quantity);

                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    EmailId = emailId,
                    ShopId = shopGroup.Key.ShopId,
                    ShopName = shopGroup.Key.ShopName,
                    ShopContactNumber = shopGroup.Key.ShopContactNumber,
                    CreatedAt = now,
                    UpdatedAt = now,
                    OrderNumber = $"{now:yyyyMMdd}-{shopGroup.Key.ShopNumber.ToString("D2")}-{count}",

                    Status = OrderStatus.ORDER_PLACED,

                    CustomerMobileNumber = mobileNumber,
                    Wing = wing,
                    FlatNumber = flatNumber,
                    Society = string.IsNullOrWhiteSpace(societyAddress) ? location.Name : societyAddress,

                    BillingAddress = $"Flat no.: {flatNumber}, Wing: {wing}, Society: {society}",
                    ShippingAddress = $"Flat no.: {flatNumber}, Wing: {wing}, Society: {society}",

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
                        ProductDescription = p.ProductDescription,
                        Quantity = p.Quantity,
                        UnitPrice = p.FinalPrice,
                        TotalPrice = p.FinalPrice * p.Quantity,
                        Type = p.Type,
                        ImageFileName = p.ImageFileName,
                        PackSize = p.PackSize,
                        Unit = p.Unit
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

        public async Task<string> Cancel(Guid id)
        {
            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id);
            if (order == null || order.Status != OrderStatus.ORDER_PLACED)
                return string.Empty;

            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            order.Status = OrderStatus.CANCELLED;
            order.UpdatedAt = now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.CANCELLED,
                CreatedAt = now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = await _context.SaveChangesAsync();
            return rowsUpdated > 0 ? order.OrderNumber : string.Empty;
        }

        public async Task<string> Accept(Guid id, Guid shopId)
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null || order.Status != OrderStatus.ORDER_PLACED)
                return string.Empty;

            order.Status = OrderStatus.ACCEPTED;
            order.UpdatedAt = now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.ACCEPTED,
                CreatedAt = now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = await _context.SaveChangesAsync();
            return rowsUpdated > 0 ? order.OrderNumber : string.Empty;
        }

        public async Task<string> Reject(Guid id, Guid shopId)
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone).Date;
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null || order.Status != OrderStatus.ORDER_PLACED)
                return string.Empty;

            order.Status = OrderStatus.REJECTED;
            order.UpdatedAt = now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.REJECTED,
                CreatedAt = now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = await _context.SaveChangesAsync();
            return rowsUpdated > 0 ? order.OrderNumber : string.Empty;
        }

        public async Task<string> Processing(Guid id, Guid shopId)
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");

            DateTime today = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone).Date;
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null || order.Status != OrderStatus.ACCEPTED)
                return string.Empty;

            order.Status = OrderStatus.PROCESSING;
            order.UpdatedAt = now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.PROCESSING,
                CreatedAt = now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = await _context.SaveChangesAsync();
            return rowsUpdated > 0 ? order.OrderNumber : string.Empty;
        }

        public async Task<string> OutForDelivery(Guid id, Guid shopId)
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null || order.Status != OrderStatus.PROCESSING)
                return string.Empty;

            order.Status = OrderStatus.OUT_FOR_DELIVERY;
            order.UpdatedAt = now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.OUT_FOR_DELIVERY,
                CreatedAt = now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = await _context.SaveChangesAsync();
            return rowsUpdated > 0 ? order.OrderNumber : string.Empty;
        }

        public async Task<string> Delivered(Guid id, Guid shopId)
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null || order.Status != OrderStatus.OUT_FOR_DELIVERY)
                return string.Empty;

            order.Status = OrderStatus.DELIVERED;
            order.UpdatedAt = now;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.DELIVERED,
                CreatedAt = now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = await _context.SaveChangesAsync();
            return rowsUpdated > 0 ? order.OrderNumber : string.Empty;
        }

        public async Task<string> PreOrder(Guid id, Guid shopId)
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == id && x.ShopId == shopId);
            if (order == null)
                return string.Empty;

            order.UpdatedAt = now;
            order.IsPreOrder = true;

            OrderTracking tracking = new OrderTracking()
            {
                Id = Guid.NewGuid(),
                OrderId = order.Id,
                Status = OrderStatus.PREORDER,
                CreatedAt = now
            };

            _context.orderTrackings.Add(tracking);

            int rowsUpdated = await _context.SaveChangesAsync();
            return rowsUpdated > 0 ? order.OrderNumber : string.Empty;
        }

        public async Task<bool> SaveToken(string emailId, string token, string role)
        {
            if (string.IsNullOrEmpty(emailId) || string.IsNullOrEmpty(token) || string.IsNullOrEmpty(role))
                return false;

            token = token.Trim(); // Remove accidental whitespace

            var existing = await _context.UserDevices
                .FirstOrDefaultAsync(x => x.EmailId == emailId && x.FcmToken == token);

            if (existing != null)
                return false; // Token already exists

            // Remove all old tokens for this user
            var oldTokens = await _context.UserDevices
                .Where(x => x.EmailId == emailId)
                .ToListAsync();

            _context.UserDevices.RemoveRange(oldTokens);

            // Add the new token
            _context.UserDevices.Add(new UserDevice
            {
                EmailId = emailId,
                Role = role,
                FcmToken = token
            });

            int rowsUpdated = 0;
            try
            {
                rowsUpdated = await _context.SaveChangesAsync();
                Console.WriteLine($"FCM token saved for {emailId}: {token}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving FCM token: {ex.Message}");
            }

            return rowsUpdated > 0;
        }

        public async Task<List<string>> GetToken(Guid orderID)
        {
            return await (
                from o in _context.Orders.AsNoTracking()
                join d in _context.UserDevices.AsNoTracking()
                on o.EmailId equals d.EmailId
                where o.Id == orderID
                select d.FcmToken).ToListAsync();
        }

        public async Task<List<ShopkeeperNotificationViewModel>> GetShopkeeperTokens(List<Guid> orderIds)
        {
            return await (
                from order in _context.Orders
                join shop in _context.Shops
                    on order.ShopId equals shop.Id
                join device in _context.UserDevices
                    on shop.OwnerEmailId equals device.EmailId
                where orderIds.Contains(order.Id)
                select new ShopkeeperNotificationViewModel
                {
                    FcmToken = device.FcmToken,
                    OrderNumber = order.OrderNumber
                }).Distinct().ToListAsync();
        }

        public async Task<string> GetSuperAdminTokens()
        {
            var isAdminNotificationAvailable = await _context.Flags.FirstOrDefaultAsync(x => x.Key.Equals("AdminNotificationAvailable", StringComparison.OrdinalIgnoreCase));

            if (isAdminNotificationAvailable == null || isAdminNotificationAvailable.Value.Equals("false", StringComparison.InvariantCultureIgnoreCase))
                return string.Empty;

            var token = await (
                from device in _context.UserDevices
                where device.EmailId == "shubham.chaudhari06@gmail.com"
                select device.FcmToken).FirstOrDefaultAsync();

            if (token == null)
                return string.Empty;

            return token;
        }

        public async Task<List<Order>> GetAllOrdersForAdmin()
        {
            List<Order> orders = new List<Order>();
            orders = await _context.Orders.Include(x => x.OrderItems).OrderBy(x => x.CreatedAt).ToListAsync();
            return orders;
        }

        public async Task DeleteOrder(Guid orderId)
        {
            Order? order = await _context.Orders.FirstOrDefaultAsync(x => x.Id == orderId);
            if (order != null)
            {
                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteOlderOrders()
        {
            TimeZoneInfo istZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Kolkata");
            DateTime now = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, istZone);

            var oldOrders = await _context.Orders.Where(o => o.CreatedAt < now.AddDays(_oldOrdersDays)).ToListAsync();

            if (oldOrders.Any())
            {
                _context.Orders.RemoveRange(oldOrders);
                await _context.SaveChangesAsync();
            }
        }
    }
}
