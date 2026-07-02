using localshopyNew.Constants;
using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    [Authorize(Roles = RoleConstant.Admin)]
    public class FlagController(IFlagService flagService) : Controller
    {
        private readonly IFlagService _flagService = flagService;

        public async Task<IActionResult> Index()
        {
            var flags = await _flagService.GetAllFlags();
            return View(flags);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(Flag model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.EditFlag = model;

                return View("Index", await _flagService.GetAllFlags());
            }

            bool isSaved = await _flagService.SaveFlag(model);
            if (isSaved)
                TempData["Success"] = "Flag saved successfully.";
            else
                ViewData["ErrorMessage"] = "Flag NOT saved.";

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string key)
        {
            var isDeleted = await _flagService.DeleteFlag(key);

            if (isDeleted)
                TempData["Success"] = "Flag deleted successfully.";
            else
                ViewData["ErrorMessage"] = "Flag NOT deleted.";

            return RedirectToAction(nameof(Index));
        }
    }
}
