using localshopyNew.Constants;
using localshopyNew.Models;
using localshopyNew.Services;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]

    public class LocationController : Controller
    {
        private readonly ILocationService _service;
        private readonly FirebaseNotificationService _firebaseService;

        public LocationController(ILocationService service, FirebaseNotificationService firebaseService)
        {
            _service = service;
            _firebaseService = firebaseService;
        }

        public async Task<IActionResult> Index()
        {
            var locations = await _service.GetActiveLocations();
            return View(locations);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Location location)
        {
            if (string.IsNullOrEmpty(location.Name))
                return View(location);

            bool isNameExists = await _service.IsLocationNameExists(location.Name);
            if (isNameExists)
            {
                ViewData["ErrorMessage"] = "Location Name already exists";
                return View(location);
            }
            bool isAdded = await _service.AddLocation(location);
            if (isAdded)
            {
                List<ShopkeeperNotificationViewModel> tokens = await _service.GetShopkeeperTokens();

                foreach (ShopkeeperNotificationViewModel token in tokens)
                {
                    await _firebaseService.SendNotificationAsync(token.FcmToken, "New Location Added", $"New location: {location.Name} added.");
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["ErrorMessage"] = "Location Not Added";
            return View(location);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var location = await _service.GetLocationById(id);
            if (location == null) return RedirectToAction(nameof(Index));
            return View(location);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Location model)
        {
            if (ModelState.IsValid)
            {
                var existsingLocation = await _service.GetLocationById(model.Id);
                if (existsingLocation == null) return RedirectToAction(nameof(Index));

                if (existsingLocation.Name != model.Name)
                {

                    bool isNameExists = await _service.IsLocationNameExists(model.Name);
                    if (isNameExists)
                    {
                        ViewData["ErrorMessage"] = "Location Name already exists";
                        return View(model);
                    }
                }
                await _service.UpdateLocation(model);
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        public async Task<IActionResult> Delete(Guid id)
        {
            var location = await _service.GetLocationById(id);
            if (location == null) return RedirectToAction(nameof(Index));
            return View(location);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var location = await _service.GetLocationById(id);
            if (location != null)
            {
                location.IsActive = false;
                bool isUpdated = await _service.UpdateLocation(location);
                if (isUpdated)
                {
                    return RedirectToAction(nameof(Index));
                }
            }
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Deleted()
        {
            var locations = await _service.GetInActiveLocations();
            return View(locations);
        }

        [HttpPost, ActionName("Restore")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(Guid id)
        {
            var location = await _service.GetLocationById(id);
            if (location != null)
            {
                location.IsActive = true;
                bool isUpdated = await _service.UpdateLocation(location);
                if (isUpdated)
                {
                    return RedirectToAction(nameof(Deleted));
                }
            }
            return RedirectToAction(nameof(Deleted));
        }

        [HttpPost, ActionName("RemoveFromDatabase")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveFromDatabase(Guid id)
        {
            bool isDeleted = await _service.DeleteLocation(id);
            if (isDeleted)
            {
                return RedirectToAction(nameof(Deleted));
            }
            ViewData["ErrorMessage"] = "Location Not Deleted";
            return RedirectToAction(nameof(Deleted));
        }
    }
}
