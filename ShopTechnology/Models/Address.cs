using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Address
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string StreetAddress { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Apartment { get; set; }

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Country { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;

        public AddressType Type { get; set; } = AddressType.Shipping;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public virtual ApplicationUser User { get; set; } = null!;
    }

    public enum AddressType
    {
        Shipping,
        Billing,
        Both
    }
}
