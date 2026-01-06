using localshopyNew.Constants;
using localshopyNew.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace localshopyNew.Controllers
{
    [Authorize(Roles = RoleConstants.Admin)]
    public class ReviewController : Controller
    {
        private readonly IReviewService _service;

        public ReviewController(IReviewService service)
        {
            _service = service;
        }

        // Pending Reviews
        public async Task<IActionResult> Index()
        {
            var reviews = await _service.GetPendingReviews();
            return View(reviews);
        }

        // Approved Reviews
        public async Task<IActionResult> Approved()
        {
            var reviews = await _service.GetApprovedReviews();
            return View(reviews);
        }

        // Rejected Reviews
        public async Task<IActionResult> Rejected()
        {
            var reviews = await _service.GetRejectedReviews();
            return View(reviews);
        }

        // Approve
        [HttpPost]
        public async Task<IActionResult> Approve(Guid id)
        {
            await _service.ApproveReview(id);
            return Redirect(Request.Headers["Referer"].ToString());
        }

        // Reject
        [HttpPost]
        public async Task<IActionResult> Reject(Guid id)
        {
            await _service.RejectReview(id);
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}
