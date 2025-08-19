using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.DTOs;

public class ReviewDTO
{
    public int ReviewId { get; set; }
    public Guid UserId { get; set; }
    public string UserFullName { get; set; } = string.Empty;
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    
    [Range(1, 5)]
    public int Rating { get; set; }
    
    [StringLength(1000)]
    public string Comment { get; set; } = string.Empty;
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Computed properties
    public string RatingDisplay => new string('★', Rating) + new string('☆', 5 - Rating);
    public string TimeAgo => GetTimeAgo(CreatedAt);

    private static string GetTimeAgo(DateTime dateTime)
    {
        var timeSpan = DateTime.Now - dateTime;
        
        if (timeSpan.TotalDays > 365)
            return $"{(int)(timeSpan.TotalDays / 365)} năm trước";
        if (timeSpan.TotalDays > 30)
            return $"{(int)(timeSpan.TotalDays / 30)} tháng trước";
        if (timeSpan.TotalDays > 7)
            return $"{(int)(timeSpan.TotalDays / 7)} tuần trước";
        if (timeSpan.TotalDays >= 1)
            return $"{(int)timeSpan.TotalDays} ngày trước";
        if (timeSpan.TotalHours >= 1)
            return $"{(int)timeSpan.TotalHours} giờ trước";
        if (timeSpan.TotalMinutes >= 1)
            return $"{(int)timeSpan.TotalMinutes} phút trước";
        
        return "Vừa xong";
    }
}

public class CreateReviewDTO
{
    [Required]
    public int ProductId { get; set; }
    
    [Required]
    [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1-5 sao")]
    public int Rating { get; set; }
    
    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Bình luận phải từ 10-1000 ký tự")]
    public string Comment { get; set; } = string.Empty;
}

public class UpdateReviewDTO
{
    [Required]
    [Range(1, 5, ErrorMessage = "Đánh giá phải từ 1-5 sao")]
    public int Rating { get; set; }
    
    [Required]
    [StringLength(1000, MinimumLength = 10, ErrorMessage = "Bình luận phải từ 10-1000 ký tự")]
    public string Comment { get; set; } = string.Empty;
}

public class ProductReviewSummaryDTO
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int FiveStarCount { get; set; }
    public int FourStarCount { get; set; }
    public int ThreeStarCount { get; set; }
    public int TwoStarCount { get; set; }
    public int OneStarCount { get; set; }
    
    // Computed properties
    public string AverageRatingDisplay => new string('★', (int)Math.Round(AverageRating)) + new string('☆', 5 - (int)Math.Round(AverageRating));
    public double FiveStarPercentage => TotalReviews > 0 ? (double)FiveStarCount / TotalReviews * 100 : 0;
    public double FourStarPercentage => TotalReviews > 0 ? (double)FourStarCount / TotalReviews * 100 : 0;
    public double ThreeStarPercentage => TotalReviews > 0 ? (double)ThreeStarCount / TotalReviews * 100 : 0;
    public double TwoStarPercentage => TotalReviews > 0 ? (double)TwoStarCount / TotalReviews * 100 : 0;
    public double OneStarPercentage => TotalReviews > 0 ? (double)OneStarCount / TotalReviews * 100 : 0;
}
