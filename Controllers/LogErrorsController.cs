using localshopyNew.Constants;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    public class LogErrorsController : Controller
    {
        private readonly ILogErrorsService _service;

        public LogErrorsController(ILogErrorsService service)
        {
            _service = service;
        }

        public async Task<IActionResult> Index()
        {

            var errors = await _service.ErrorList();
            return View(errors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                bool isDeleted = await _service.Delete(id);
                return Json(new { success = isDeleted });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message); // helpful for debugging
            }
        }
    }
}
