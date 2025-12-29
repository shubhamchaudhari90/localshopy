using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class ProductMasterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
