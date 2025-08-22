using ShopTechnology.Models;

namespace ShopTechnology.Services;

public interface IReviewService
{
    Task<List<Review>> GetReviewsByProductIdAsync(int productId, int page = 1, int pageSize = 10);
    Task<Review?> GetReviewByIdAsync(int reviewId);
    Task<Review?> GetUserReviewForProductAsync(Guid userId, int productId);
    Task<Review> CreateReviewAsync(Review review);
    Task<bool> UpdateReviewAsync(Review review);
    Task<bool> DeleteReviewAsync(int reviewId);
    Task<List<Review>> GetReviewsByUserIdAsync(Guid userId);
    Task<double> GetAverageRatingForProductAsync(int productId);
    Task<int> GetReviewCountForProductAsync(int productId);
    Task<Dictionary<int, double>> GetAverageRatingsForProductsAsync(List<int> productIds);
    Task<List<Review>> GetRecentReviewsAsync(int count = 10);
    Task<List<Review>> GetTopRatedReviewsAsync(int productId, int count = 5);
    Task<List<Review>> GetLowRatedReviewsAsync(int productId, int count = 5);
    Task<bool> HasUserPurchasedProductAsync(Guid userId, int productId);
    Task<bool> HasUserReviewedProductAsync(Guid userId, int productId);
    Task<Dictionary<int, int>> GetRatingDistributionAsync(int productId);
    Task<List<Review>> SearchReviewsAsync(string searchTerm, int? productId = null);
    Task<bool> ValidateReviewAsync(Guid userId, int productId);
    Task<int> GetTotalReviewsCountAsync();
    Task<double> GetOverallAverageRatingAsync();
    Task<List<Review>> GetReviewsByRatingAsync(int productId, int rating, int page = 1, int pageSize = 10);
}
