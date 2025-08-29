using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetProductReviewsAsync(int productId, bool approvedOnly = true);
        Task<Review?> GetReviewByIdAsync(int reviewId);
        Task<bool> CreateReviewAsync(Review review);
        Task<bool> UpdateReviewAsync(Review review);
        Task<bool> DeleteReviewAsync(int reviewId);
        Task<bool> ApproveReviewAsync(int reviewId);
        Task<bool> RejectReviewAsync(int reviewId);
        Task<decimal> GetAverageRatingAsync(int productId);
        Task<int> GetReviewCountAsync(int productId);
        Task<IEnumerable<Review>> GetUserReviewsAsync(string userId);
        Task<bool> MarkReviewAsHelpfulAsync(int reviewId);
        Task<bool> IsUserVerifiedPurchaseAsync(string userId, int productId);
        Task<IEnumerable<Review>> GetPendingReviewsAsync();
        Task<bool> AddReviewImageAsync(int reviewId, string imageUrl);
        Task<bool> RemoveReviewImageAsync(int imageId);
    }
}
