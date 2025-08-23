using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.Services;
using System.Security.Claims;

namespace ShopTechnology.Controllers
{
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IUserService _userService;

        public ReviewController(IReviewService reviewService, IUserService userService)
        {
            _reviewService = reviewService;
            _userService = userService;
        }

        [HttpPost]
        public async Task<IActionResult> AddReview(int productId, int rating, string title, string comment)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để đánh giá sản phẩm." });
            }

            // Validate review
            if (!await _reviewService.ValidateReviewAsync(userId.Value, productId))
            {
                return Json(new { success = false, message = "Bạn chỉ có thể đánh giá sản phẩm đã mua và chưa đánh giá." });
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = userId.Value,
                Rating = rating,
                Title = title,
                Comment = comment,
                IsApproved = false, // Admin needs to approve
                CreatedAt = DateTime.Now
            };

            await _reviewService.CreateReviewAsync(review);

            return Json(new { success = true, message = "Đánh giá của bạn đã được gửi và đang chờ phê duyệt." });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateReview(int reviewId, int rating, string title, string comment)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var review = await _reviewService.GetReviewByIdAsync(reviewId);
            if (review == null || review.UserId != userId.Value)
            {
                return Json(new { success = false, message = "Đánh giá không tồn tại." });
            }

            review.Rating = rating;
            review.Title = title;
            review.Comment = comment;
            review.IsApproved = false; // Reset approval status
            review.UpdatedAt = DateTime.Now;

            await _reviewService.UpdateReviewAsync(review);

            return Json(new { success = true, message = "Đánh giá đã được cập nhật và đang chờ phê duyệt." });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var review = await _reviewService.GetReviewByIdAsync(reviewId);
            if (review == null || review.UserId != userId.Value)
            {
                return Json(new { success = false, message = "Đánh giá không tồn tại." });
            }

            await _reviewService.DeleteReviewAsync(reviewId);

            return Json(new { success = true, message = "Đánh giá đã được xóa." });
        }

        [HttpGet]
        public async Task<IActionResult> GetProductReviews(int productId, int page = 1)
        {
            var reviews = await _reviewService.GetReviewsByProductIdAsync(productId, page, 10);
            var averageRating = await _reviewService.GetAverageRatingForProductAsync(productId);
            var reviewCount = await _reviewService.GetReviewCountForProductAsync(productId);

            return Json(new
            {
                reviews = reviews.Select(r => new
                {
                    r.ReviewId,
                    r.Rating,
                    r.Title,
                    r.Comment,
                    r.CreatedAt,
                    UserName = r.User?.FullName ?? "Anonymous",
                    r.IsVerified
                }),
                averageRating,
                reviewCount,
                page
            });
        }

        [HttpPost]
        public async Task<IActionResult> MarkHelpful(int reviewId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var review = await _reviewService.GetReviewByIdAsync(reviewId);
            if (review == null)
            {
                return Json(new { success = false, message = "Đánh giá không tồn tại." });
            }

            review.HelpfulCount++;
            await _reviewService.UpdateReviewAsync(review);

            return Json(new { success = true, helpfulCount = review.HelpfulCount });
        }

        [HttpPost]
        public async Task<IActionResult> MarkUnhelpful(int reviewId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập." });
            }

            var review = await _reviewService.GetReviewByIdAsync(reviewId);
            if (review == null)
            {
                return Json(new { success = false, message = "Đánh giá không tồn tại." });
            }

            review.UnhelpfulCount++;
            await _reviewService.UpdateReviewAsync(review);

            return Json(new { success = true, unhelpfulCount = review.UnhelpfulCount });
        }

        [HttpGet]
        public async Task<IActionResult> GetUserReviews(int page = 1)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account");
            }

            var reviews = await _reviewService.GetReviewsByUserIdAsync(userId.Value);
            return View(reviews);
        }

        [HttpGet]
        public async Task<IActionResult> GetRatingDistribution(int productId)
        {
            var distribution = await _reviewService.GetRatingDistributionAsync(productId);
            return Json(distribution);
        }

        private int? GetUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : null;
        }
    }
}
