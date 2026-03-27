using localshopyNew.Models;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    public class BlockedUserController(IBlockedUserService blockedUserService) : Controller
    {
        private readonly IBlockedUserService _blockedUserService = blockedUserService;

        public async Task<IActionResult> Index()
        {
            List<BlockedUser> blockedUsers = await _blockedUserService.GetBlockedUsers();
            return View(blockedUsers);
        }

        [HttpPost]
        public async Task<IActionResult> Add([FromBody] BlockedUser model)
        {
            List<BlockedUser> blockedUsers = new List<BlockedUser>();
            var isAdded = await _blockedUserService.AddBlockedUser(model);
            if (isAdded)
            {
                blockedUsers = await _blockedUserService.GetBlockedUsers();
                return Json(blockedUsers);
            }
            return Json(blockedUsers);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(string emailId)
        {
            await _blockedUserService.RemoveBlockedUser(emailId);
            return Ok();
        }
    }
}
