using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.DTOs;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class ReviewService : IReviewService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IMapper _mapper;

    public ReviewService(ShopTechnologyAccessoriesContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<ReviewDTO>> GetAllReviewsAsync()
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ReviewDTO>>(reviews);
    }

    public async Task<ReviewDTO?> GetReviewByIdAsync(int id)
    {
        var review = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .FirstOrDefaultAsync(r => r.ReviewId == id);

        return _mapper.Map<ReviewDTO>(review);
    }

    public async Task<List<ReviewDTO>> GetReviewsByProductIdAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.ProductId == productId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ReviewDTO>>(reviews);
    }

    public async Task<List<ReviewDTO>> GetReviewsByUserIdAsync(Guid userId)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<ReviewDTO>>(reviews);
    }

    public async Task<ReviewDTO> CreateReviewAsync(Guid userId, CreateReviewDTO createReviewDto)
    {
        // Check if user has already reviewed this product
        if (await HasUserReviewedProductAsync(userId, createReviewDto.ProductId))
        {
            throw new InvalidOperationException("Bạn đã đánh giá sản phẩm này rồi");
        }

        // Check if user has purchased this product (for verified badge)
        var isVerified = await HasUserPurchasedProductAsync(userId, createReviewDto.ProductId);

        var review = _mapper.Map<Review>(createReviewDto);
        review.UserId = userId;
        review.IsVerified = isVerified;
        review.CreatedAt = DateTime.UtcNow;

        _context.Reviews.Add(review);
        await _context.SaveChangesAsync();

        return await GetReviewByIdAsync(review.ReviewId) ?? throw new InvalidOperationException("Failed to create review");
    }

    public async Task<ReviewDTO> UpdateReviewAsync(int reviewId, Guid userId, UpdateReviewDTO updateReviewDto)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == userId);

        if (review == null)
        {
            throw new InvalidOperationException("Review not found or you don't have permission to edit it");
        }

        _mapper.Map(updateReviewDto, review);
        review.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetReviewByIdAsync(reviewId) ?? throw new InvalidOperationException("Failed to update review");
    }

    public async Task<bool> DeleteReviewAsync(int reviewId, Guid userId)
    {
        var review = await _context.Reviews
            .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == userId);

        if (review == null)
        {
            return false;
        }

        _context.Reviews.Remove(review);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> VerifyReviewAsync(int reviewId)
    {
        var review = await _context.Reviews.FindAsync(reviewId);
        if (review == null)
        {
            return false;
        }

        review.IsVerified = true;
        review.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<ProductReviewSummaryDTO> GetProductReviewSummaryAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        var summary = new ProductReviewSummaryDTO
        {
            ProductId = productId,
            TotalReviews = reviews.Count,
            AverageRating = reviews.Any() ? reviews.Average(r => r.Rating) : 0,
            FiveStarCount = reviews.Count(r => r.Rating == 5),
            FourStarCount = reviews.Count(r => r.Rating == 4),
            ThreeStarCount = reviews.Count(r => r.Rating == 3),
            TwoStarCount = reviews.Count(r => r.Rating == 2),
            OneStarCount = reviews.Count(r => r.Rating == 1)
        };

        // Get product name
        var product = await _context.Products.FindAsync(productId);
        if (product != null)
        {
            summary.ProductName = product.ProductName;
        }

        return summary;
    }

    public async Task<double> GetProductAverageRatingAsync(int productId)
    {
        var reviews = await _context.Reviews
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        return reviews.Any() ? reviews.Average(r => r.Rating) : 0;
    }

    public async Task<int> GetProductReviewCountAsync(int productId)
    {
        return await _context.Reviews
            .CountAsync(r => r.ProductId == productId);
    }

    public async Task<bool> HasUserReviewedProductAsync(Guid userId, int productId)
    {
        return await _context.Reviews
            .AnyAsync(r => r.UserId == userId && r.ProductId == productId);
    }

    public async Task<bool> HasUserPurchasedProductAsync(Guid userId, int productId)
    {
        // Check if user has any completed orders containing this product
        return await _context.OrderDetails
            .Include(od => od.Order)
            .AnyAsync(od => od.Order.UserId == userId && 
                           od.ProductId == productId && 
                           od.Order.Status == "Completed");
    }

    public async Task<List<ReviewDTO>> GetRecentReviewsAsync(int count = 10)
    {
        var reviews = await _context.Reviews
            .Include(r => r.User)
            .Include(r => r.Product)
            .OrderByDescending(r => r.CreatedAt)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<List<ReviewDTO>>(reviews);
    }

    public async Task<int> GetTotalReviewsCountAsync()
    {
        return await _context.Reviews.CountAsync();
    }
}
