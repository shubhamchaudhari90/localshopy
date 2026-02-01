using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Instructions()
        {
            return View();
        }
    }
}
