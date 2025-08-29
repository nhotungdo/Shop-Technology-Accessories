using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Contact
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        [Required]
        [MaxLength(200)]
        public string Subject { get; set; } = string.Empty;

        [Required]
        [MaxLength(2000)]
        public string Message { get; set; } = string.Empty;

        public ContactStatus Status { get; set; } = ContactStatus.New;

        public ContactType Type { get; set; } = ContactType.General;

        public string? AssignedToUserId { get; set; }

        [MaxLength(1000)]
        public string? AdminResponse { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? RespondedAt { get; set; }

        public DateTime? ResolvedAt { get; set; }

        // Navigation property
        public virtual ApplicationUser? AssignedToUser { get; set; }
    }

    public enum ContactStatus
    {
        New,
        InProgress,
        Resolved,
        Closed
    }

    public enum ContactType
    {
        General,
        Technical,
        Billing,
        Shipping,
        Complaint,
        Suggestion
    }
}
