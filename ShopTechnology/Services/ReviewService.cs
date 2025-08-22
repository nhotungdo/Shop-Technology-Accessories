using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class ReviewService : IReviewService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly ILogger<ReviewService> _logger;

    public ReviewService(ShopTechnologyAccessoriesContext context, ILogger<ReviewService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Review>> GetReviewsByProductIdAsync(int productId, int page = 1, int pageSize = 10)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Review?> GetReviewByIdAsync(int reviewId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
    }

    public async Task<Review?> GetUserReviewForProductAsync(Guid userId, int productId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);
    }

    public async Task<Review> CreateReviewAsync(Review review)
    {
        review.CreatedAt = DateTime.UtcNow;

        _context.Reviews.Add(review);

        // Update product rating
        await UpdateProductRatingAsync(review.ProductId);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Review created for product {ProductId} by user {UserId}", review.ProductId, review.UserId);
        return review;
    }

    public async Task<bool> UpdateReviewAsync(Review review)
    {
        try
        {
            var existingReview = await _context.Reviews.FindAsync(review.ReviewId);
            if (existingReview == null)
            {
                return false;
            }

            existingReview.Rating = review.Rating;
            existingReview.Comment = review.Comment;
            existingReview.UpdatedAt = DateTime.UtcNow;

            // Update product rating
            await UpdateProductRatingAsync(review.ProductId);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Review updated: {ReviewId}", review.ReviewId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review: {ReviewId}", review.ReviewId);
            return false;
        }
    }

    public async Task<bool> DeleteReviewAsync(int reviewId)
    {
        try
        {
            var review = await _context.Reviews.FindAsync(reviewId);
            if (review == null)
            {
                return false;
            }

            var productId = review.ProductId;
            _context.Reviews.Remove(review);

            // Update product rating
            await UpdateProductRatingAsync(productId);

            await _context.SaveChangesAsync();

            _logger.LogInformation("Review deleted: {ReviewId}", reviewId);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review: {ReviewId}", reviewId);
            return false;
        }
    }

    public async Task<List<Review>> GetReviewsByUserIdAsync(Guid userId)
    {
        return await _context.Reviews
            .Include(r => r.Product)
            .ThenInclude(p => p.ProductImages)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<double> GetAverageRatingForProductAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        if (!reviews.Any())
        {
            return 0;
        }

        return reviews.Average(r => r.Rating);
    }

    public async Task<int> GetReviewCountForProductAsync(int productId)
    {
        return await _context.Reviews
            .CountAsync(r => r.ProductId == productId);
    }

    public async Task<Dictionary<int, double>> GetAverageRatingsForProductsAsync(List<int> productIds)
    {
        var ratings = await _context.Reviews
            .Where(r => productIds.Contains(r.ProductId))
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, AverageRating = g.Average(r => r.Rating) })
            .ToListAsync();

        return ratings.ToDictionary(r => r.ProductId, r => r.AverageRating);
    }

    public async Task<List<Review>> GetRecentReviewsAsync(int count = 10)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Review>> GetTopRatedReviewsAsync(int productId, int count = 5)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.Rating >= 4)
            .OrderByDescending(r => r.Rating)
            .ThenByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Review>> GetLowRatedReviewsAsync(int productId, int count = 5)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.Rating <= 2)
            .OrderBy(r => r.Rating)
            .ThenByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> HasUserPurchasedProductAsync(Guid userId, int productId)
    {
        return await _context.OrderDetails
            .Include(od => od.Order)
            .AnyAsync(od => od.ProductId == productId && 
                           od.Order.UserId == userId && 
                           od.Order.Status == "Completed");
    }

    public async Task<bool> HasUserReviewedProductAsync(Guid userId, int productId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int productId)
    {
        var distribution = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .GroupBy(r => r.Rating)
            .Select(g => new { Rating = g.Key, Count = g.Count() })
            .ToListAsync();

        var result = new Dictionary<int, int>();
        for (int i = 1; i <= 5; i++)
        {
            result[i] = distribution.FirstOrDefault(d => d.Rating == i)?.Count ?? 0;
        }

        return result;
    }

    public async Task<List<Review>> SearchReviewsAsync(string searchTerm, int? productId = null)
    {
        var query = _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .AsQueryable();

        if (productId.HasValue)
        {
            query = query.Where(r => r.ProductId == productId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            query = query.Where(r => r.Comment.Contains(searchTerm) || 
                                   r.User.FullName.Contains(searchTerm));
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ValidateReviewAsync(Guid userId, int productId)
    {
        // Check if user has purchased the product
        var hasPurchased = await HasUserPurchasedProductAsync(userId, productId);
        if (!hasPurchased)
        {
            return false;
        }

        // Check if user has already reviewed the product
        var hasReviewed = await HasUserReviewedProductAsync(userId, productId);
        if (hasReviewed)
        {
            return false;
        }

        return true;
    }

    private async Task UpdateProductRatingAsync(int productId)
    {
        try
        {
            var reviews = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .ToListAsync();

            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                if (reviews.Any())
                {
                    product.Rating = reviews.Average(r => r.Rating);
                    product.ReviewCount = reviews.Count;
                }
                else
                {
                    product.Rating = 0;
                    product.ReviewCount = 0;
                }

                product.UpdatedAt = DateTime.UtcNow;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product rating for product {ProductId}", productId);
        }
    }

    public async Task<int> GetTotalReviewsCountAsync()
    {
        return await _context.Reviews.CountAsync();
    }

    public async Task<double> GetOverallAverageRatingAsync()
    {
        var reviews = await _context.Reviews.ToListAsync();
        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }

    public async Task<List<Review>> GetReviewsByRatingAsync(int productId, int rating, int page = 1, int pageSize = 10)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.Rating == rating)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
