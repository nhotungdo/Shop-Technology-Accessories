using System.ComponentModel.DataAnnotations;

namespace ShopTechnology.Models
{
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties - removed to avoid conflict
        // public virtual ICollection<User> Users { get; set; }
    }
}
