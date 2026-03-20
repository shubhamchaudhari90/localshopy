using localshopyNew.Data;
using localshopyNew.Services.Interfaces;
using localshopyNew.ViewModel;
using Microsoft.EntityFrameworkCore;

namespace localshopyNew.Services
{
    public class ReviewService : IReviewService
    {
        private readonly AppDBContext _context;

        public ReviewService(AppDBContext context)
        {
            _context = context;
        }

        public async Task<List<ReviewViewModel>?> GetPendingReviews()
        {
            var reviews =
            await (
            from review in _context.Reviews

            join product in _context.Products
            on review.ProductId equals product.Id

            join shop in _context.Shops
            on product.ShopId equals shop.Id

            join pm in _context.ProductMasters
            on product.ProductMasterId equals pm.Id

            where !review.IsApproved && !review.IsRejected

            select new ReviewViewModel
            {
                Id = review.Id,
                Comment = review.Comment,
                IsApproved = review.IsApproved,
                IsRejected = review.IsRejected,
                Rating = review.Rating,
                ProductId = product.Id,
                ProductName = pm.ProductName,
                ShopName = shop.Name
            }).ToListAsync();

            return reviews;
        }

        public async Task<List<ReviewViewModel>?> GetApprovedReviews()
        {
            var reviews =
            await (
            from review in _context.Reviews

            join product in _context.Products
            on review.ProductId equals product.Id

            join shop in _context.Shops
            on product.ShopId equals shop.Id

            join pm in _context.ProductMasters
            on product.ProductMasterId equals pm.Id

            where review.IsApproved && !review.IsRejected

            select new ReviewViewModel
            {
                Id = review.Id,
                Comment = review.Comment,
                IsApproved = review.IsApproved,
                IsRejected = review.IsRejected,
                Rating = review.Rating,
                ProductId = product.Id,
                ProductName = pm.ProductName,
                ShopName = shop.Name
            }).ToListAsync();

            return reviews;
        }

        public async Task<List<ReviewViewModel>?> GetRejectedReviews()
        {
            var reviews =
            await (
            from review in _context.Reviews

            join product in _context.Products
            on review.ProductId equals product.Id

            join shop in _context.Shops
            on product.ShopId equals shop.Id

            join pm in _context.ProductMasters
            on product.ProductMasterId equals pm.Id

            where !review.IsApproved && review.IsRejected

            select new ReviewViewModel
            {
                Id = review.Id,
                Comment = review.Comment,
                IsApproved = review.IsApproved,
                IsRejected = review.IsRejected,
                Rating = review.Rating,
                ProductId = product.Id,
                ProductName = pm.ProductName,
                ShopName = shop.Name
            }).ToListAsync();

            return reviews;
        }

        public async Task<bool> ApproveReview(Guid reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
                return false;

            if (review.IsApproved)
                return true;

            review.IsApproved = true;
            review.IsRejected = false;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> RejectReview(Guid reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
                return false;

            if (review.IsRejected)
                return true;

            review.IsApproved = false;
            review.IsRejected = true;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteReview(Guid reviewId)
        {
            var review = await _context.Reviews.FindAsync(reviewId);

            if (review == null)
                return false;

            if (review.IsRejected)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                return true;
            }
            return false;
        }
    }
}
