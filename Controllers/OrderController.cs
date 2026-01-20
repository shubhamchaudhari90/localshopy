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
        public async Task<IActionResult> OrderDetails(string wing, string flatNumber)
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
                    return View(orders);
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

        public async Task<IActionResult> OrderDetails(Guid id)
        {
            var order = await _orderService.GetOrder(id);

            if (order is null)
                return NotFound();

            return View(new List<Order> { order });
        }


        public IActionResult Shopkeeper()
        {

            return View();
        }
    }
}
