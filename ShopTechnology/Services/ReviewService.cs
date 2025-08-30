using Microsoft.EntityFrameworkCore;
using ShopTechnology.Data;
using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetProductReviewsAsync(int productId, bool approvedOnly = true)
        {
            var query = _context.Reviews
                .Include(r => r.User)
                .Include(r => r.ReviewImages)
                .Where(r => r.ProductId == productId);

            return await query
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<Review?> GetReviewByIdAsync(int reviewId)
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.ReviewImages)
                .FirstOrDefaultAsync(r => r.ReviewId == reviewId);
        }

        public async Task<bool> CreateReviewAsync(Review review)
        {
            try
            {
                review.CreatedAt = DateTime.UtcNow;
                _context.Reviews.Add(review);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateReviewAsync(Review review)
        {
            try
            {
                review.UpdatedAt = DateTime.UtcNow;
                _context.Reviews.Update(review);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null) return false;

                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ApproveReviewAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null) return false;

                review.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RejectReviewAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null) return false;

                review.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<decimal> GetAverageRatingAsync(int productId)
        {
            var average = await _context.Reviews
                .Where(r => r.ProductId == productId)
                .AverageAsync(r => r.Rating);
            return Math.Round((decimal)average, 1);
        }

        public async Task<int> GetReviewCountAsync(int productId)
        {
            return await _context.Reviews
                .CountAsync(r => r.ProductId == productId);
        }

        public async Task<IEnumerable<Review>> GetUserReviewsAsync(string userId)
        {
            return await _context.Reviews
                .Include(r => r.ReviewImages)
                .Where(r => r.UserId == int.Parse(userId))
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> MarkReviewAsHelpfulAsync(int reviewId)
        {
            try
            {
                var review = await _context.Reviews.FindAsync(reviewId);
                if (review == null) return false;

                review.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> IsUserVerifiedPurchaseAsync(string userId, int productId)
        {
            return await _context.OrderDetails
                .AnyAsync(od => od.Order.UserId == int.Parse(userId) &&
                               od.ProductId == productId &&
                               od.Order.OrderStatus == "Delivered");
        }

        public async Task<IEnumerable<Review>> GetPendingReviewsAsync()
        {
            return await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> AddReviewImageAsync(int reviewId, string imageUrl)
        {
            try
            {
                var reviewImage = new ReviewImage
                {
                    ReviewId = reviewId,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.UtcNow
                };

                _context.ReviewImages.Add(reviewImage);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> RemoveReviewImageAsync(int imageId)
        {
            try
            {
                var image = await _context.ReviewImages.FindAsync(imageId);
                if (image == null) return false;

                _context.ReviewImages.Remove(image);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
