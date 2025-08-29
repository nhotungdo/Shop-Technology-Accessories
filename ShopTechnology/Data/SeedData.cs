using Microsoft.AspNetCore.Identity;
using ShopTechnology.Models;

namespace ShopTechnology.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            // Seed roles
            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }

            if (!await roleManager.RoleExistsAsync("User"))
            {
                await roleManager.CreateAsync(new IdentityRole("User"));
            }

            // Seed admin user
            var adminEmail = "admin@shoptechnology.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FirstName = "Admin",
                    LastName = "User",
                    EmailConfirmed = true,
                    PhoneNumberConfirmed = true,
                    IsActive = true
                };

                var result = await userManager.CreateAsync(adminUser, "Admin123!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // Seed categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category
                    {
                        Name = "Laptops",
                        Description = "High-performance laptops for work and gaming",
                        Slug = "laptops",
                        DisplayOrder = 1,
                        IsActive = true,
                        MetaTitle = "Laptops - Shop Technology",
                        MetaDescription = "Find the best laptops for your needs"
                    },
                    new Category
                    {
                        Name = "Smartphones",
                        Description = "Latest smartphones with advanced features",
                        Slug = "smartphones",
                        DisplayOrder = 2,
                        IsActive = true,
                        MetaTitle = "Smartphones - Shop Technology",
                        MetaDescription = "Discover the latest smartphones"
                    },
                    new Category
                    {
                        Name = "Accessories",
                        Description = "Essential tech accessories",
                        Slug = "accessories",
                        DisplayOrder = 3,
                        IsActive = true,
                        MetaTitle = "Tech Accessories - Shop Technology",
                        MetaDescription = "Quality tech accessories for all devices"
                    }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();

                // Add subcategories
                var laptopCategory = categories[0];
                var smartphoneCategory = categories[1];

                var subCategories = new List<Category>
                {
                    new Category
                    {
                        Name = "Gaming Laptops",
                        Description = "High-performance gaming laptops",
                        Slug = "gaming-laptops",
                        ParentId = laptopCategory.Id,
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new Category
                    {
                        Name = "Business Laptops",
                        Description = "Professional business laptops",
                        Slug = "business-laptops",
                        ParentId = laptopCategory.Id,
                        DisplayOrder = 2,
                        IsActive = true
                    },
                    new Category
                    {
                        Name = "Android Phones",
                        Description = "Android smartphones",
                        Slug = "android-phones",
                        ParentId = smartphoneCategory.Id,
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new Category
                    {
                        Name = "iPhone",
                        Description = "Apple iPhone smartphones",
                        Slug = "iphone",
                        ParentId = smartphoneCategory.Id,
                        DisplayOrder = 2,
                        IsActive = true
                    }
                };

                context.Categories.AddRange(subCategories);
                await context.SaveChangesAsync();
            }

            // Seed products
            if (!context.Products.Any())
            {
                var gamingLaptopCategory = context.Categories.FirstOrDefault(c => c.Slug == "gaming-laptops");
                var businessLaptopCategory = context.Categories.FirstOrDefault(c => c.Slug == "business-laptops");
                var androidCategory = context.Categories.FirstOrDefault(c => c.Slug == "android-phones");
                var iphoneCategory = context.Categories.FirstOrDefault(c => c.Slug == "iphone");

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Gaming Laptop Pro X",
                        Description = "High-performance gaming laptop with RTX 4080",
                        LongDescription = "Experience ultimate gaming performance with our Gaming Laptop Pro X. Featuring the latest RTX 4080 graphics card, 32GB RAM, and 1TB NVMe SSD.",
                        SKU = "GLP-X-001",
                        Price = 2999.99m,
                        CompareAtPrice = 3499.99m,
                        StockQuantity = 50,
                        CategoryId = gamingLaptopCategory?.Id ?? 1,
                        Brand = "TechPro",
                        Model = "GLP-X",
                        Slug = "gaming-laptop-pro-x",
                        Weight = 2.5m,
                        Length = 35.5m,
                        Width = 24.5m,
                        Height = 2.2m,
                        IsActive = true,
                        IsFeatured = true,
                        IsNew = true,
                        IsHot = true,
                        MetaTitle = "Gaming Laptop Pro X - Shop Technology",
                        MetaDescription = "High-performance gaming laptop with RTX 4080"
                    },
                    new Product
                    {
                        Name = "Business UltraBook",
                        Description = "Professional business laptop for productivity",
                        LongDescription = "Perfect for business professionals. Lightweight, powerful, and secure with enterprise-grade security features.",
                        SKU = "BUL-001",
                        Price = 1299.99m,
                        StockQuantity = 75,
                        CategoryId = businessLaptopCategory?.Id ?? 1,
                        Brand = "BusinessTech",
                        Model = "UltraBook",
                        Slug = "business-ultrabook",
                        Weight = 1.8m,
                        Length = 32.0m,
                        Width = 22.0m,
                        Height = 1.5m,
                        IsActive = true,
                        IsFeatured = true,
                        MetaTitle = "Business UltraBook - Shop Technology",
                        MetaDescription = "Professional business laptop for productivity"
                    },
                    new Product
                    {
                        Name = "Android Galaxy Pro",
                        Description = "Latest Android smartphone with advanced camera",
                        LongDescription = "Capture stunning photos with the 108MP camera system. 5G ready with 256GB storage.",
                        SKU = "AGP-001",
                        Price = 899.99m,
                        StockQuantity = 100,
                        CategoryId = androidCategory?.Id ?? 2,
                        Brand = "Android",
                        Model = "Galaxy Pro",
                        Slug = "android-galaxy-pro",
                        Weight = 0.2m,
                        Length = 16.5m,
                        Width = 7.5m,
                        Height = 0.8m,
                        IsActive = true,
                        IsNew = true,
                        MetaTitle = "Android Galaxy Pro - Shop Technology",
                        MetaDescription = "Latest Android smartphone with advanced camera"
                    },
                    new Product
                    {
                        Name = "iPhone 15 Pro",
                        Description = "Apple's latest flagship smartphone",
                        LongDescription = "Experience the future with iPhone 15 Pro. A17 Pro chip, titanium design, and advanced camera system.",
                        SKU = "IP15P-001",
                        Price = 1199.99m,
                        StockQuantity = 60,
                        CategoryId = iphoneCategory?.Id ?? 2,
                        Brand = "Apple",
                        Model = "iPhone 15 Pro",
                        Slug = "iphone-15-pro",
                        Weight = 0.187m,
                        Length = 14.7m,
                        Width = 7.1m,
                        Height = 0.8m,
                        IsActive = true,
                        IsFeatured = true,
                        IsHot = true,
                        MetaTitle = "iPhone 15 Pro - Shop Technology",
                        MetaDescription = "Apple's latest flagship smartphone"
                    }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();

                // Add product images
                var productImages = new List<ProductImage>
                {
                    new ProductImage
                    {
                        ProductId = products[0].Id,
                        ImageUrl = "/img/products/gaming-laptop-1.jpg",
                        AltText = "Gaming Laptop Pro X",
                        DisplayOrder = 1,
                        IsMain = true
                    },
                    new ProductImage
                    {
                        ProductId = products[1].Id,
                        ImageUrl = "/img/products/business-laptop-1.jpg",
                        AltText = "Business UltraBook",
                        DisplayOrder = 1,
                        IsMain = true
                    },
                    new ProductImage
                    {
                        ProductId = products[2].Id,
                        ImageUrl = "/img/products/android-phone-1.jpg",
                        AltText = "Android Galaxy Pro",
                        DisplayOrder = 1,
                        IsMain = true
                    },
                    new ProductImage
                    {
                        ProductId = products[3].Id,
                        ImageUrl = "/img/products/iphone-15-pro-1.jpg",
                        AltText = "iPhone 15 Pro",
                        DisplayOrder = 1,
                        IsMain = true
                    }
                };

                context.ProductImages.AddRange(productImages);
                await context.SaveChangesAsync();

                // Add product specifications
                var specifications = new List<ProductSpecification>
                {
                    // Gaming Laptop specs
                    new ProductSpecification { ProductId = products[0].Id, Name = "Processor", Value = "Intel Core i9-13900H", DisplayOrder = 1, IsHighlighted = true },
                    new ProductSpecification { ProductId = products[0].Id, Name = "Graphics", Value = "NVIDIA RTX 4080 16GB", DisplayOrder = 2, IsHighlighted = true },
                    new ProductSpecification { ProductId = products[0].Id, Name = "RAM", Value = "32GB DDR5", DisplayOrder = 3 },
                    new ProductSpecification { ProductId = products[0].Id, Name = "Storage", Value = "1TB NVMe SSD", DisplayOrder = 4 },
                    new ProductSpecification { ProductId = products[0].Id, Name = "Display", Value = "15.6\" 4K OLED", DisplayOrder = 5 },

                    // Business Laptop specs
                    new ProductSpecification { ProductId = products[1].Id, Name = "Processor", Value = "Intel Core i7-1370P", DisplayOrder = 1, IsHighlighted = true },
                    new ProductSpecification { ProductId = products[1].Id, Name = "Graphics", Value = "Intel Iris Xe", DisplayOrder = 2 },
                    new ProductSpecification { ProductId = products[1].Id, Name = "RAM", Value = "16GB LPDDR5", DisplayOrder = 3 },
                    new ProductSpecification { ProductId = products[1].Id, Name = "Storage", Value = "512GB NVMe SSD", DisplayOrder = 4 },
                    new ProductSpecification { ProductId = products[1].Id, Name = "Display", Value = "14\" 2K IPS", DisplayOrder = 5 },

                    // Android Phone specs
                    new ProductSpecification { ProductId = products[2].Id, Name = "Processor", Value = "Snapdragon 8 Gen 2", DisplayOrder = 1, IsHighlighted = true },
                    new ProductSpecification { ProductId = products[2].Id, Name = "Camera", Value = "108MP + 12MP + 10MP", DisplayOrder = 2, IsHighlighted = true },
                    new ProductSpecification { ProductId = products[2].Id, Name = "RAM", Value = "12GB", DisplayOrder = 3 },
                    new ProductSpecification { ProductId = products[2].Id, Name = "Storage", Value = "256GB", DisplayOrder = 4 },
                    new ProductSpecification { ProductId = products[2].Id, Name = "Display", Value = "6.7\" AMOLED 120Hz", DisplayOrder = 5 },

                    // iPhone specs
                    new ProductSpecification { ProductId = products[3].Id, Name = "Processor", Value = "A17 Pro", DisplayOrder = 1, IsHighlighted = true },
                    new ProductSpecification { ProductId = products[3].Id, Name = "Camera", Value = "48MP + 12MP + 12MP", DisplayOrder = 2, IsHighlighted = true },
                    new ProductSpecification { ProductId = products[3].Id, Name = "RAM", Value = "8GB", DisplayOrder = 3 },
                    new ProductSpecification { ProductId = products[3].Id, Name = "Storage", Value = "256GB", DisplayOrder = 4 },
                    new ProductSpecification { ProductId = products[3].Id, Name = "Display", Value = "6.1\" Super Retina XDR", DisplayOrder = 5 }
                };

                context.ProductSpecifications.AddRange(specifications);
                await context.SaveChangesAsync();
            }

            // Seed banners
            if (!context.Banners.Any())
            {
                var banners = new List<Banner>
                {
                    new Banner
                    {
                        Title = "New Gaming Laptops",
                        Description = "Discover the latest gaming laptops with RTX 4080",
                        ImageUrl = "/img/banners/gaming-banner.jpg",
                        LinkUrl = "/products?category=gaming-laptops",
                        ButtonText = "Shop Now",
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new Banner
                    {
                        Title = "iPhone 15 Pro",
                        Description = "Experience the future with iPhone 15 Pro",
                        ImageUrl = "/img/banners/iphone-banner.jpg",
                        LinkUrl = "/products?category=iphone",
                        ButtonText = "Learn More",
                        DisplayOrder = 2,
                        IsActive = true
                    }
                };

                context.Banners.AddRange(banners);
                await context.SaveChangesAsync();
            }

            // Seed FAQs
            if (!context.FAQs.Any())
            {
                var faqs = new List<FAQ>
                {
                    new FAQ
                    {
                        Question = "How do I track my order?",
                        Answer = "You can track your order by logging into your account and visiting the 'My Orders' section. You'll receive tracking updates via email as well.",
                        Category = "Shipping",
                        DisplayOrder = 1,
                        IsActive = true
                    },
                    new FAQ
                    {
                        Question = "What payment methods do you accept?",
                        Answer = "We accept credit cards, debit cards, bank transfers, and various e-wallets including VNPay, Momo, and ZaloPay.",
                        Category = "Payment",
                        DisplayOrder = 2,
                        IsActive = true
                    },
                    new FAQ
                    {
                        Question = "Do you offer warranty on products?",
                        Answer = "Yes, all our products come with manufacturer warranty. The warranty period varies by product and brand.",
                        Category = "Warranty",
                        DisplayOrder = 3,
                        IsActive = true
                    }
                };

                context.FAQs.AddRange(faqs);
                await context.SaveChangesAsync();
            }

            // Seed promotions
            if (!context.Promotions.Any())
            {
                var promotions = new List<Promotion>
                {
                    new Promotion
                    {
                        Name = "Welcome Discount",
                        Description = "Get 10% off your first order",
                        Code = "WELCOME10",
                        Type = PromotionType.Percentage,
                        Value = 10,
                        MinimumOrderAmount = 100,
                        MaximumDiscountAmount = 50,
                        UsageLimit = 1,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddYears(1),
                        IsActive = true,
                        IsFirstTimeOnly = true
                    },
                    new Promotion
                    {
                        Name = "Free Shipping",
                        Description = "Free shipping on orders over $200",
                        Code = "FREESHIP",
                        Type = PromotionType.FreeShipping,
                        Value = 0,
                        MinimumOrderAmount = 200,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddYears(1),
                        IsActive = true
                    }
                };

                context.Promotions.AddRange(promotions);
                await context.SaveChangesAsync();
            }
        }
    }
}
