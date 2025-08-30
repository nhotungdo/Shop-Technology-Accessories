using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Core entities
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }

        // Order management
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<OrderHistory> OrderHistories { get; set; }

        // Cart and checkout
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        // Reviews and ratings
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ReviewImage> ReviewImages { get; set; }

        // Promotions and discounts
        public DbSet<Promotion> Promotions { get; set; }
        public DbSet<ProductPromotion> ProductPromotions { get; set; }

        // Wishlist
        public DbSet<Wishlist> Wishlists { get; set; }

        // Content management
        public DbSet<Banner> Banners { get; set; }
        public DbSet<FAQ> FAQs { get; set; }
        public DbSet<Contact> Contacts { get; set; }

        // Payment
        public DbSet<Payment> Payments { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure relationships and constraints

            // User and Role relationships
            builder.Entity<User>()
                .HasOne(u => u.Role)
                .WithMany(r => r.Users)
                .HasForeignKey(u => u.RoleId)
                .OnDelete(DeleteBehavior.Restrict);

            // Category self-referencing relationship
            builder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product relationships
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.ProductImages)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order relationships
            builder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(od => od.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderDetail>()
                .HasOne(od => od.Product)
                .WithMany(p => p.OrderDetails)
                .HasForeignKey(od => od.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<OrderHistory>()
                .HasOne(oh => oh.Order)
                .WithMany(o => o.OrderHistories)
                .HasForeignKey(oh => oh.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<OrderHistory>()
                .HasOne(oh => oh.UpdatedByUser)
                .WithMany()
                .HasForeignKey(oh => oh.UpdatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Cart relationships
            builder.Entity<Cart>()
                .HasOne(c => c.User)
                .WithMany(u => u.Carts)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.CartItems)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CartItem>()
                .HasOne(ci => ci.Product)
                .WithMany(p => p.CartItems)
                .HasForeignKey(ci => ci.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review relationships
            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ReviewImage>()
                .HasOne(ri => ri.Review)
                .WithMany(r => r.ReviewImages)
                .HasForeignKey(ri => ri.ReviewId)
                .OnDelete(DeleteBehavior.Cascade);

            // Promotion relationships
            builder.Entity<ProductPromotion>()
                .HasOne(pp => pp.Product)
                .WithMany(p => p.ProductPromotions)
                .HasForeignKey(pp => pp.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProductPromotion>()
                .HasOne(pp => pp.Promotion)
                .WithMany(p => p.ProductPromotions)
                .HasForeignKey(pp => pp.PromotionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Wishlist relationship
            builder.Entity<Wishlist>()
                .HasOne(w => w.User)
                .WithMany(u => u.Wishlists)
                .HasForeignKey(w => w.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Wishlist>()
                .HasOne(w => w.Product)
                .WithMany(p => p.Wishlists)
                .HasForeignKey(w => w.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Payment relationship
            builder.Entity<Payment>()
                .HasOne(p => p.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(p => p.OrderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Contact relationship
            builder.Entity<Contact>()
                .HasOne(c => c.RepliedByUser)
                .WithMany()
                .HasForeignKey(c => c.RepliedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure indexes
            builder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            builder.Entity<User>()
                .HasIndex(u => u.PhoneNumber);

            builder.Entity<Product>()
                .HasIndex(p => p.SKU)
                .IsUnique();

            builder.Entity<Product>()
                .HasIndex(p => p.Slug);

            builder.Entity<Category>()
                .HasIndex(c => c.Slug);

            builder.Entity<Promotion>()
                .HasIndex(p => p.Code)
                .IsUnique();

            builder.Entity<Order>()
                .HasIndex(o => o.OrderNumber)
                .IsUnique();

            builder.Entity<Cart>()
                .HasIndex(c => c.SessionId)
                .IsUnique();

            // Configure decimal precision
            builder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.OriginalPrice)
                .HasPrecision(18, 2);

            builder.Entity<Product>()
                .Property(p => p.AverageRating)
                .HasPrecision(3, 2);

            builder.Entity<Order>()
                .Property(o => o.SubTotal)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.TaxAmount)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.ShippingFee)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.DiscountAmount)
                .HasPrecision(18, 2);

            builder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            builder.Entity<OrderDetail>()
                .Property(od => od.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<OrderDetail>()
                .Property(od => od.TotalPrice)
                .HasPrecision(18, 2);

            builder.Entity<CartItem>()
                .Property(ci => ci.UnitPrice)
                .HasPrecision(18, 2);

            builder.Entity<CartItem>()
                .Property(ci => ci.TotalPrice)
                .HasPrecision(18, 2);

            builder.Entity<Promotion>()
                .Property(p => p.DiscountValue)
                .HasPrecision(18, 2);

            builder.Entity<Promotion>()
                .Property(p => p.MinimumOrderAmount)
                .HasPrecision(18, 2);

            builder.Entity<Promotion>()
                .Property(p => p.MaximumDiscountAmount)
                .HasPrecision(18, 2);

            builder.Entity<Payment>()
                .Property(p => p.Amount)
                .HasPrecision(18, 2);
        }
    }
}
