using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public IActionResult NotFound404()
        {
            Response.StatusCode = 404;
            return View();
        }

        [Route("Error/401")]
        public IActionResult Unauthorized401()
        {
            Response.StatusCode = 401;
            return View();
        }

        [Route("Error/500")]
        public IActionResult InternalServerError()
        {
            Response.StatusCode = 500;
            return View();
        }
    }
}
