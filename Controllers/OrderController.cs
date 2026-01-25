using localshopyNew.Constants;
using localshopyNew.Models;
using localshopyNew.Services;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace localshopyNew.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly ISessionService _sessionService;
        private readonly IOrderService _orderService;
        private readonly FirebaseNotificationService _notification;
        public OrderController(ISessionService sessionService, IOrderService orderService)
        {
            _sessionService = sessionService;
            _orderService = orderService;
            _notification = new FirebaseNotificationService();
        }

        [HttpPost]
        public async Task<IActionResult> OrderDetails(Guid id)
        {
            var order = await _orderService.GetOrderById(id);

            if (order is null)
                return NotFound();
            if (order.Status == OrderStatus.DELIVERED)
                TempData["Order"] = "TRUE";
            return View(new List<Order> { order });
        }

        public async Task<IActionResult> Index()
        {
            List<Order> ordersList = new List<Order>();
            string? email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                Guid locationId = _sessionService.GetLocation();
                if (locationId != Guid.Empty)
                {
                    ordersList = await _orderService.GetAllOrders(email, locationId);
                    return View(ordersList);
                }
            }
            return View(ordersList);
        }

        public async Task<IActionResult> AllOrders()
        {
            List<Order> orderList = new List<Order>();
            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }
            orderList = await _orderService.GetAllOrdersByShopId(shopId);
            return View(orderList);
        }

        public async Task<IActionResult> OrdersToServe()
        {
            List<Order> orderList = new List<Order>();
            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                return RedirectToAction("Logout", "Account");
            }
            orderList = await _orderService.OrdersToServe(shopId);
            return View(orderList);
        }

        [HttpPost]
        public async Task<IActionResult> ProcessOrder(Guid id)
        {
            var order = await _orderService.GetOrderById(id);

            if (order is null)
                return NotFound();

            return View(new List<Order> { order });
        }

        [HttpPost]
        public async Task<IActionResult> PlaceOrder(string wing, string flatNumber)
        {
            TempData["ErrorMessage"] = null;
            string? email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                Guid locationId = _sessionService.GetLocation();
                if (locationId != Guid.Empty)
                {
                    List<Order> orders = await _orderService.PlaceOrder(email, locationId, flatNumber, wing);

                    List<Guid> orderIds = orders.Select(x => x.Id).ToList();

                    List<string> shopkeeperTokens = await _orderService.GetShopkeeperTokens(orderIds);

                    foreach (string shopkeeperToken in shopkeeperTokens)
                    {
                        await _notification.SendNotificationAsync(shopkeeperToken, "New Order", "You have received a new order");
                    }
                    _sessionService.SetCartCount(0);
                    return View("OrderDetails", orders);
                }
            }

            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(Guid id)
        {
            TempData["ErrorMessage"] = null;
            bool isCancelled = await _orderService.Cancel(id);
            if (!isCancelled)
            {
                TempData["ErrorMessage"] = "Order is not cancelled";
            }
            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(Guid id)
        {
            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = null;
            bool isRejected = await _orderService.Reject(id, shopId);
            if (!isRejected)
            {
                TempData["ErrorMessage"] = "Order is not Rejected";
            }
            else
            {
                await SendUserNotification("Order Update", OrderStatus.REJECTED, id);
            }

            return RedirectToAction(nameof(OrdersToServe));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Accept(Guid id)
        {
            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = null;
            bool isAccepted = await _orderService.Accept(id, shopId);
            if (!isAccepted)
            {
                TempData["ErrorMessage"] = "Order is not Accepted";
            }
            else
            {
                await SendUserNotification("Order Update", OrderStatus.ACCEPTED, id);
            }
            return RedirectToAction(nameof(OrdersToServe));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Processing(Guid id)
        {
            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = null;
            bool isAccepted = await _orderService.Processing(id, shopId);
            if (!isAccepted)
            {
                TempData["ErrorMessage"] = "Order is not Processing";
            }
            else
            {
                await SendUserNotification("Order Update", OrderStatus.PROCESSING, id);
            }
            return RedirectToAction(nameof(OrdersToServe));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OutForDelivery(Guid id)
        {
            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = null;
            bool isAccepted = await _orderService.OutForDelivery(id, shopId);
            if (!isAccepted)
            {
                TempData["ErrorMessage"] = "Order is not OutForDelivery";
            }
            else
            {
                await SendUserNotification("Order Update", OrderStatus.OUT_FOR_DELIVERY, id);
            }
            return RedirectToAction(nameof(OrdersToServe));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delivered(Guid id)
        {
            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = null;
            bool isAccepted = await _orderService.Delivered(id, shopId);
            if (!isAccepted)
            {
                TempData["ErrorMessage"] = "Order is not Delivered";
            }
            else
            {
                await SendUserNotification("Order Update", OrderStatus.DELIVERED, id);
            }
            return RedirectToAction(nameof(AllOrders));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PreOrder(Guid id)
        {
            Guid shopId = _sessionService.GetShopId();
            if (shopId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            TempData["ErrorMessage"] = null;
            bool isAccepted = await _orderService.PreOrder(id, shopId);
            if (!isAccepted)
            {
                TempData["ErrorMessage"] = "Order is able to PRE-ORDER";
            }
            return RedirectToAction(nameof(AllOrders));
        }

        [HttpPost]
        public async Task<IActionResult> SaveFcmToken([FromBody] string token)
        {
            string emailId = User.FindFirst(ClaimTypes.Email)?.Value;
            string role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (!string.IsNullOrEmpty(emailId) && !string.IsNullOrEmpty(token))
            {
                if (string.IsNullOrEmpty(role))
                    role = "User";
                bool isSaved = await _orderService.SaveToken(emailId, token, role);
            }


            return Ok();
        }

        public async Task SendUserNotification(string type, string orderStatus, Guid orderID)
        {
            try
            {

                List<string> userTokens = await _orderService.GetToken(orderID);

                foreach (var userToken in userTokens)
                {
                    await _notification.SendNotificationAsync(userToken, type, $"Your order status is now {orderStatus}");
                }

            }
            catch (Exception e)
            {

            }
        }

    }
}
