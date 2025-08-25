using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Contact
    {
        [Key]
        public int ContactId { get; set; }

        [Required]
        [StringLength(100)]
        public string FullName { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; }

        [StringLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [StringLength(200)]
        public string Subject { get; set; }

        [Required]
        [StringLength(1000)]
        public string Message { get; set; }

        [StringLength(50)]
        public string Status { get; set; } = "New"; // New, Replied, Closed

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? RepliedAt { get; set; }

        [StringLength(1000)]
        public string? ReplyMessage { get; set; }

        public int? RepliedByUserId { get; set; }

        // Navigation properties
        public virtual User? RepliedByUser { get; set; }
    }
}
