using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ShopTechnology.Models
{
    public class User
    {
        [Key]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [MaxLength(255)]
        public string? Address { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(100)]
        public string? Province { get; set; }

        [MaxLength(20)]
        public string? PostalCode { get; set; }

        [Required]
        public DateTime DateOfBirth { get; set; }

        [MaxLength(255)]
        public string? Avatar { get; set; }

        public bool IsEmailVerified { get; set; } = false;

        public bool IsPhoneVerified { get; set; } = false;

        [MaxLength(255)]
        public string? EmailVerificationToken { get; set; }

        public DateTime? EmailVerificationExpiry { get; set; }

        [MaxLength(255)]
        public string? PasswordResetToken { get; set; }

        public DateTime? PasswordResetExpiry { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string? SocialLoginProvider { get; set; }

        [MaxLength(255)]
        public string? SocialLoginId { get; set; }

        // Navigation properties
        [ForeignKey("RoleId")]
        public virtual Role Role { get; set; } = null!;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<Wishlist> Wishlists { get; set; } = new List<Wishlist>();
        public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();
        public virtual ICollection<Contact> Contacts { get; set; } = new List<Contact>();
        public virtual ICollection<OrderHistory> OrderHistories { get; set; } = new List<OrderHistory>();
    }
}
