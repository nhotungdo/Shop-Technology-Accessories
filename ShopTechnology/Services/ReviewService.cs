using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class ReviewService : IReviewService
{
    private readonly ShopTechnologyAccessoriesContext _context;

    public ReviewService(ShopTechnologyAccessoriesContext context)
    {
        _context = context;
    }

    public async Task<List<Review>> GetReviewsByProductIdAsync(int productId, int page = 1, int pageSize = 10)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.ReviewImages)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<Review?> GetReviewByIdAsync(int reviewId)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.ReviewImages)
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
    }

    public async Task<Review?> GetUserReviewForProductAsync(int userId, int productId)
    {
        return await _context.Reviews
            .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);
    }

    public async Task<Review> CreateReviewAsync(Review review)
    {
        review.CreatedAt = DateTime.Now;
        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();
        return review;
    }

    public async Task<bool> UpdateReviewAsync(Review review)
    {
        var existingReview = await _context.Reviews.FindAsync(review.ReviewId);
        if (existingReview == null) return false;

        existingReview.Rating = review.Rating;
        existingReview.Title = review.Title;
        existingReview.Comment = review.Comment;
        existingReview.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReviewAsync(int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null) return false;

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Review>> GetReviewsByUserIdAsync(int userId)
    {
        return await _context.Reviews
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<double> GetAverageRatingForProductAsync(int productId)
    {
        var average = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
            .AverageAsync(r => (double)r.Rating);
        return Math.Round(average, 2);
    }

    public async Task<int> GetReviewCountForProductAsync(int productId)
    {
        return await _context.Reviews
            .CountAsync(r => r.ProductId == productId && r.IsApproved);
    }

    public async Task<Dictionary<int, double>> GetAverageRatingsForProductsAsync(List<int> productIds)
    {
        var ratings = await _context.Reviews
            .Where(r => productIds.Contains(r.ProductId) && r.IsApproved)
            .GroupBy(r => r.ProductId)
            .Select(g => new { ProductId = g.Key, AverageRating = g.Average(r => (double)r.Rating) })
            .ToListAsync();

        return ratings.ToDictionary(r => r.ProductId, r => Math.Round(r.AverageRating, 2));
    }

    public async Task<List<Review>> GetRecentReviewsAsync(int count = 10)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Review>> GetTopRatedReviewsAsync(int productId, int count = 5)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderByDescending(r => r.Rating)
            .ThenByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<List<Review>> GetLowRatedReviewsAsync(int productId, int count = 5)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.IsApproved)
            .OrderBy(r => r.Rating)
            .ThenByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();
    }

    public async Task<bool> HasUserPurchasedProductAsync(int userId, int productId)
    {
        return await _context.OrderDetails
            .AnyAsync(od => od.Order.UserId == userId && od.ProductId == productId);
    }

    public async Task<bool> HasUserReviewedProductAsync(int userId, int productId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
    }

    public async Task<Dictionary<int, int>> GetRatingDistributionAsync(int productId)
    {
        var distribution = await _context.Reviews
            .Where(r => r.ProductId == productId && r.IsApproved)
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
            .Where(r => r.IsApproved && 
                       (r.Title.Contains(searchTerm) || r.Comment.Contains(searchTerm)));

        if (productId.HasValue)
        {
            query = query.Where(r => r.ProductId == productId.Value);
        }

        return await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();
    }

    public async Task<bool> ValidateReviewAsync(int userId, int productId)
    {
        // Kiểm tra xem user đã mua sản phẩm chưa
        var hasPurchased = await HasUserPurchasedProductAsync(userId, productId);
        if (!hasPurchased) return false;

        // Kiểm tra xem user đã review sản phẩm chưa
        var hasReviewed = await HasUserReviewedProductAsync(userId, productId);
        if (hasReviewed) return false;

        return true;
    }

    public async Task<int> GetTotalReviewsCountAsync()
    {
        return await _context.Reviews.CountAsync(r => r.IsApproved);
    }

    public async Task<double> GetOverallAverageRatingAsync()
    {
        var average = await _context.Reviews
            .Where(r => r.IsApproved)
            .AverageAsync(r => (double)r.Rating);
        return Math.Round(average, 2);
    }

    public async Task<List<Review>> GetReviewsByRatingAsync(int productId, int rating, int page = 1, int pageSize = 10)
    {
        return await _context.Reviews
            .Include(r => r.User)
            .Where(r => r.ProductId == productId && r.Rating == rating && r.IsApproved)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
