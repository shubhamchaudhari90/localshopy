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
        public OrderController(ISessionService sessionService)
        {
            _sessionService = sessionService;
        }
        public IActionResult Customer()
        {

            string? email = User.FindFirstValue(ClaimTypes.Email);
            if (!string.IsNullOrEmpty(email))
            {
                Guid locationId = _sessionService.GetLocation();

            }

            return View();
        }

        public IActionResult Shopkeeper()
        {

            return View();
        }
    }
}
