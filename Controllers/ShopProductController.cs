using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class ShopProductController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
