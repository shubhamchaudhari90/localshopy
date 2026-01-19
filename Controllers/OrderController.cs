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
            string? email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                Guid locationId = _sessionService.GetLocation();
                if (locationId != Guid.Empty)
                {
                    List<Order> orders = await _orderService.PlaceOrder(email, locationId, flatNumber, wing);
                    int cartCount = _sessionService.GetCartCount();
                    foreach (Order order in orders)
                    {
                        int itemsCount = order.OrderItems.Sum(x => x.Quantity);
                        cartCount = cartCount - itemsCount;
                    }
                    _sessionService.SetCartCount(cartCount);
                    return View(orders);
                }
            }

            return RedirectToAction("Index", "Cart");
        }

        public async Task<IActionResult> Cancel(Guid id)
        {

            return RedirectToAction("Index", "Order");
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

            if (order == null) return NotFound();

            List<Order> orders = new List<Order>();

            orders.Add(order);

            return View(orders);
        }


        public IActionResult Shopkeeper()
        {

            return View();
        }
    }
}
