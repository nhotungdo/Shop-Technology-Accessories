using ShopTechnology.DTOs;

namespace ShopTechnology.Services;

public interface IReviewService
{
    Task<List<ReviewDTO>> GetAllReviewsAsync();
    Task<ReviewDTO?> GetReviewByIdAsync(int id);
    Task<List<ReviewDTO>> GetReviewsByProductIdAsync(int productId);
    Task<List<ReviewDTO>> GetReviewsByUserIdAsync(Guid userId);
    Task<ReviewDTO> CreateReviewAsync(Guid userId, CreateReviewDTO createReviewDto);
    Task<ReviewDTO> UpdateReviewAsync(int reviewId, Guid userId, UpdateReviewDTO updateReviewDto);
    Task<bool> DeleteReviewAsync(int reviewId, Guid userId);
    Task<bool> VerifyReviewAsync(int reviewId);
    Task<ProductReviewSummaryDTO> GetProductReviewSummaryAsync(int productId);
    Task<double> GetProductAverageRatingAsync(int productId);
    Task<int> GetProductReviewCountAsync(int productId);
    Task<bool> HasUserReviewedProductAsync(Guid userId, int productId);
    Task<bool> HasUserPurchasedProductAsync(Guid userId, int productId);
    Task<List<ReviewDTO>> GetRecentReviewsAsync(int count = 10);
    Task<int> GetTotalReviewsCountAsync();
}
