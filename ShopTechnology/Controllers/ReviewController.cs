using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using System.Security.Claims;

namespace ShopTechnology.Controllers;

[Authorize]
public class ReviewController : Controller
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly ILogger<ReviewController> _logger;

    public ReviewController(
        ShopTechnologyAccessoriesContext context,
        ILogger<ReviewController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> AddReview(int productId, int rating, string comment)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            // Validate rating
            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Đánh giá phải từ 1-5 sao" });
            }

            // Check if user has already reviewed this product
            var existingReview = await _context.Reviews
                .FirstOrDefaultAsync(r => r.UserId == userId && r.ProductId == productId);

            if (existingReview != null)
            {
                return Json(new { success = false, message = "Bạn đã đánh giá sản phẩm này rồi" });
            }

            // Check if user has purchased this product
            var hasPurchased = await _context.OrderDetails
                .Include(od => od.Order)
                .AnyAsync(od => od.ProductId == productId && 
                               od.Order.UserId == userId && 
                               od.Order.Status == "Completed");

            if (!hasPurchased)
            {
                return Json(new { success = false, message = "Bạn cần mua sản phẩm này để đánh giá" });
            }

            // Create review
            var review = new Review
            {
                UserId = userId.Value,
                ProductId = productId,
                Rating = rating,
                Comment = comment,
                CreatedAt = DateTime.UtcNow
            };

            _context.Reviews.Add(review);

            // Update product rating
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ProductId == productId)
                    .ToListAsync();

                product.Rating = reviews.Average(r => r.Rating);
                product.ReviewCount = reviews.Count;
                product.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Review added for product {ProductId} by user {UserId}", productId, userId);

            return Json(new { 
                success = true, 
                message = "Đánh giá đã được gửi thành công!",
                rating = product?.Rating ?? 0,
                reviewCount = product?.ReviewCount ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding review for product {ProductId}", productId);
            return Json(new { success = false, message = "Có lỗi xảy ra khi gửi đánh giá" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> UpdateReview(int reviewId, int rating, string comment)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            // Validate rating
            if (rating < 1 || rating > 5)
            {
                return Json(new { success = false, message = "Đánh giá phải từ 1-5 sao" });
            }

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == userId);

            if (review == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });
            }

            review.Rating = rating;
            review.Comment = comment;
            review.UpdatedAt = DateTime.UtcNow;

            // Update product rating
            var product = await _context.Products.FindAsync(review.ProductId);
            if (product != null)
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ProductId == review.ProductId)
                    .ToListAsync();

                product.Rating = reviews.Average(r => r.Rating);
                product.ReviewCount = reviews.Count;
                product.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Review updated for product {ProductId} by user {UserId}", review.ProductId, userId);

            return Json(new { 
                success = true, 
                message = "Đánh giá đã được cập nhật thành công!",
                rating = product?.Rating ?? 0,
                reviewCount = product?.ReviewCount ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating review {ReviewId}", reviewId);
            return Json(new { success = false, message = "Có lỗi xảy ra khi cập nhật đánh giá" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteReview(int reviewId)
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập" });
            }

            var review = await _context.Reviews
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId && r.UserId == userId);

            if (review == null)
            {
                return Json(new { success = false, message = "Không tìm thấy đánh giá" });
            }

            var productId = review.ProductId;
            _context.Reviews.Remove(review);

            // Update product rating
            var product = await _context.Products.FindAsync(productId);
            if (product != null)
            {
                var reviews = await _context.Reviews
                    .Where(r => r.ProductId == productId)
                    .ToListAsync();

                product.Rating = reviews.Any() ? reviews.Average(r => r.Rating) : 0;
                product.ReviewCount = reviews.Count;
                product.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            _logger.LogInformation("Review deleted for product {ProductId} by user {UserId}", productId, userId);

            return Json(new { 
                success = true, 
                message = "Đánh giá đã được xóa thành công!",
                rating = product?.Rating ?? 0,
                reviewCount = product?.ReviewCount ?? 0
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting review {ReviewId}", reviewId);
            return Json(new { success = false, message = "Có lỗi xảy ra khi xóa đánh giá" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetProductReviews(int productId, int page = 1)
    {
        try
        {
            const int pageSize = 10;
            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == productId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var totalCount = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .CountAsync();

            var viewModel = new ProductReviewsViewModel
            {
                Reviews = reviews,
                CurrentPage = page,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                TotalCount = totalCount
            };

            return PartialView("_ProductReviews", viewModel);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading reviews for product {ProductId}", productId);
            return Json(new { error = "Có lỗi xảy ra khi tải đánh giá" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> MyReviews()
    {
        try
        {
            var userId = GetCurrentUserId();
            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var reviews = await _context.Reviews
                .Include(r => r.Product)
                .ThenInclude(p => p.ProductImages)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reviews);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading user reviews for user {UserId}", GetCurrentUserId());
            return View("Error");
        }
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        return null;
    }
}

public class ProductReviewsViewModel
{
    public List<Review> Reviews { get; set; } = new();
    public int CurrentPage { get; set; }
    public int TotalPages { get; set; }
    public int TotalCount { get; set; }
}
