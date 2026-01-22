using localshopyNew.Constants;
using localshopyNew.Models;
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

        public OrderController(ISessionService sessionService, IOrderService orderService)
        {
            _sessionService = sessionService;
            _orderService = orderService;
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
            bool isCancelled = await _orderService.Reject(id, shopId);
            if (!isCancelled)
            {
                TempData["ErrorMessage"] = "Order is not Rejected";
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
            return RedirectToAction(nameof(AllOrders));
        }

    }
}
