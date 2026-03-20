using localshopyNew.ViewModel;

namespace localshopyNew.Services.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewViewModel>?> GetPendingReviews();
        Task<List<ReviewViewModel>?> GetApprovedReviews();
        Task<List<ReviewViewModel>?> GetRejectedReviews();
        Task<bool> ApproveReview(Guid reviewId);
        Task<bool> RejectReview(Guid reviewId);
        Task<bool> DeleteReview(Guid reviewId);
    }
}
