using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Notification
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(1000)]
        public string Message { get; set; } = string.Empty;

        public NotificationType Type { get; set; } = NotificationType.Info;

        public bool IsRead { get; set; } = false;

        [MaxLength(200)]
        public string? LinkUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ReadAt { get; set; }

        // Navigation property
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public enum NotificationType
    {
        Info,
        Success,
        Warning,
        Error,
        Order,
        Promotion
    }
}
