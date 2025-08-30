using ShopTechnology.Models;

namespace ShopTechnology.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // Seed roles
            if (!context.Roles.Any())
            {
                var roles = new List<Role>
                {
                    new Role { Name = "Admin", CreatedAt = DateTime.UtcNow },
                    new Role { Name = "User", CreatedAt = DateTime.UtcNow }
                };

                context.Roles.AddRange(roles);
                await context.SaveChangesAsync();
            }

            // Seed admin user
            var adminEmail = "donhotung2004@gmail.com";
            var adminUser = context.Users.FirstOrDefault(u => u.Email == adminEmail);

            if (adminUser == null)
            {
                var adminRole = context.Roles.FirstOrDefault(r => r.Name == "Admin");
                adminUser = new User
                {
                    RoleId = adminRole?.RoleId ?? 1,
                    FullName = "Admin",
                    Email = adminEmail,
                    PhoneNumber = "0931982568",
                    Password = "Donhotung2004", // In real app, this should be hashed
                    DateOfBirth = new DateTime(1990, 1, 1),
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Users.Add(adminUser);
                await context.SaveChangesAsync();
            }

            // Seed categories
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category
                    {
                        Name = "Sạc",
                        Description = "Sạc điện thoại, laptop, tablet",
                        Slug = "sac",
                        DisplayOrder = 1,
                        IsActive = true,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Name = "Tai nghe",
                        Description = "Tai nghe có dây, không dây",
                        Slug = "tai-nghe",
                        DisplayOrder = 2,
                        IsActive = true,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Name = "Ốp lưng",
                        Description = "Ốp bảo vệ cho điện thoại, tablet",
                        Slug = "op-lung",
                        DisplayOrder = 3,
                        IsActive = true,
                        IsFeatured = false,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Name = "Bàn phím",
                        Description = "Bàn phím cơ, bàn phím không dây",
                        Slug = "ban-phim",
                        DisplayOrder = 4,
                        IsActive = true,
                        IsFeatured = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Category
                    {
                        Name = "Chuột",
                        Description = "Chuột gaming, chuột văn phòng",
                        Slug = "chuot",
                        DisplayOrder = 5,
                        IsActive = true,
                        IsFeatured = false,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Categories.AddRange(categories);
                await context.SaveChangesAsync();
            }

            // Seed products
            if (!context.Products.Any())
            {
                var sacCategory = context.Categories.FirstOrDefault(c => c.Slug == "sac");
                var taiNgheCategory = context.Categories.FirstOrDefault(c => c.Slug == "tai-nghe");
                var banPhimCategory = context.Categories.FirstOrDefault(c => c.Slug == "ban-phim");

                var products = new List<Product>
                {
                    new Product
                    {
                        Name = "Bộ chuyển đổi USB-C",
                        Description = "Bộ chuyển đổi USB-C đa cổng với HDMI, USB 3.0 và Ethernet",
                        Price = 49.99m,
                        OriginalPrice = 59.99m,
                        Brand = "UGreen",
                        Model = "USB-C Hub",
                        SKU = "UG-USB-C-001",
                        StockQuantity = 150,
                        CategoryId = sacCategory?.CategoryId ?? 1,
                        MainImage = "https://viethansecurity.com/media/product/9507_bo_chuyen_doi_ugreen_40873_cm179.jpg",
                        IsActive = true,
                        IsFeatured = true,
                        Slug = "bo-chuyen-doi-usb-c",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Đế sạc không dây",
                        Description = "Đế sạc không dây tốc độ cao tương thích với các thiết bị hỗ trợ Qi",
                        Price = 29.99m,
                        OriginalPrice = 39.99m,
                        Brand = "Anker",
                        Model = "Wireless Charger",
                        SKU = "ANK-WIRELESS-001",
                        StockQuantity = 200,
                        CategoryId = sacCategory?.CategoryId ?? 1,
                        MainImage = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRoaacgNWVoXN3W-mMdzeDdt7HZ6_QbiiDgqLgLcJYEwzRRipOE5qIyNyvgo5CxgjvqZgI&usqp=CAU",
                        IsActive = true,
                        IsFeatured = true,
                        Slug = "de-sac-khong-day",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Bàn phím Bluetooth",
                        Description = "Bàn phím không dây nhỏ gọn với đèn nền",
                        Price = 59.99m,
                        OriginalPrice = 69.99m,
                        Brand = "Logitech",
                        Model = "BT Keyboard",
                        SKU = "LOG-BT-KB-001",
                        StockQuantity = 100,
                        CategoryId = banPhimCategory?.CategoryId ?? 4,
                        MainImage = "https://cohotech.vn/wp-content/uploads/2025/05/NuPhy-Air75-V2-va-NuPhy-Air96-V2-2.webp",
                        IsActive = true,
                        IsFeatured = true,
                        Slug = "ban-phim-bluetooth",
                        CreatedAt = DateTime.UtcNow
                    },
                    new Product
                    {
                        Name = "Tai nghe khử tiếng ồn",
                        Description = "Tai nghe không dây với công nghệ khử tiếng ồn chủ động",
                        Price = 79.99m,
                        OriginalPrice = 99.99m,
                        Brand = "Sony",
                        Model = "NC Headphones",
                        SKU = "SONY-NC-001",
                        StockQuantity = 120,
                        CategoryId = taiNgheCategory?.CategoryId ?? 2,
                        MainImage = "https://tainghe.com.vn/media/news/697_noisecancellingheadphones_1280_1519236823944_1280w.jpg",
                        IsActive = true,
                        IsFeatured = true,
                        Slug = "tai-nghe-khu-tieng-on",
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Products.AddRange(products);
                await context.SaveChangesAsync();

                // Add product images
                var productImages = new List<ProductImage>
                {
                    new ProductImage
                    {
                        ProductId = products[0].ProductId,
                        ImageUrl = "https://viethansecurity.com/media/product/9507_bo_chuyen_doi_ugreen_40873_cm179.jpg",
                        IsMain = true,
                        DisplayOrder = 1,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductImage
                    {
                        ProductId = products[0].ProductId,
                        ImageUrl = "https://www.tnc.com.vn/uploads/product/duyen2021/cable-usb-c-ugreen-40873.jpg",
                        IsMain = false,
                        DisplayOrder = 2,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductImage
                    {
                        ProductId = products[1].ProductId,
                        ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRoaacgNWVoXN3W-mMdzeDdt7HZ6_QbiiDgqLgLcJYEwzRRipOE5qIyNyvgo5CxgjvqZgI&usqp=CAU",
                        IsMain = true,
                        DisplayOrder = 1,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductImage
                    {
                        ProductId = products[2].ProductId,
                        ImageUrl = "https://cohotech.vn/wp-content/uploads/2025/05/NuPhy-Air75-V2-va-NuPhy-Air96-V2-2.webp",
                        IsMain = true,
                        DisplayOrder = 1,
                        CreatedAt = DateTime.UtcNow
                    },
                    new ProductImage
                    {
                        ProductId = products[3].ProductId,
                        ImageUrl = "https://tainghe.com.vn/media/news/697_noisecancellingheadphones_1280_1519236823944_1280w.jpg",
                        IsMain = true,
                        DisplayOrder = 1,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.ProductImages.AddRange(productImages);
                await context.SaveChangesAsync();
            }

            // Seed banners
            if (!context.Banners.Any())
            {
                var banners = new List<Banner>
                {
                    new Banner
                    {
                        Title = "Khuyến mãi mùa hè",
                        ImageUrl = "https://img.freepik.com/free-vector/special-offer-modern-sale-banner_1017-20667.jpg",
                        LinkUrl = "/promotions",
                        Position = "Home",
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Banner
                    {
                        Title = "Sản phẩm mới",
                        ImageUrl = "https://img.freepik.com/free-vector/gradient-sale-background_23-2148934475.jpg",
                        LinkUrl = "/products/new",
                        Position = "Home",
                        DisplayOrder = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
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
                        Question = "Làm thế nào để đặt hàng?",
                        Answer = "Bạn có thể đặt hàng bằng cách thêm sản phẩm vào giỏ hàng và tiến hành thanh toán.",
                        Category = "Đặt hàng",
                        DisplayOrder = 1,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new FAQ
                    {
                        Question = "Thời gian giao hàng là bao lâu?",
                        Answer = "Thời gian giao hàng từ 1-3 ngày làm việc tùy thuộc vào địa chỉ giao hàng.",
                        Category = "Giao hàng",
                        DisplayOrder = 2,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new FAQ
                    {
                        Question = "Có thể đổi trả sản phẩm không?",
                        Answer = "Có, bạn có thể đổi trả sản phẩm trong vòng 30 ngày kể từ ngày nhận hàng.",
                        Category = "Đổi trả",
                        DisplayOrder = 3,
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
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
                        Name = "Chào mừng khách hàng mới",
                        Description = "Giảm 10% cho đơn hàng đầu tiên",
                        Code = "WELCOME10",
                        DiscountType = "Percentage",
                        DiscountValue = 10.00m,
                        MinimumOrderAmount = 100000m,
                        MaximumDiscountAmount = 50000m,
                        UsageLimit = 1,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(6),
                        IsActive = true,
                        IsPublic = true,
                        CreatedAt = DateTime.UtcNow
                    },
                    new Promotion
                    {
                        Name = "Miễn phí vận chuyển",
                        Description = "Miễn phí vận chuyển cho đơn hàng từ 300k",
                        Code = "FREESHIP",
                        DiscountType = "FixedAmount",
                        DiscountValue = 50000m,
                        MinimumOrderAmount = 300000m,
                        MaximumDiscountAmount = 50000m,
                        UsageLimit = 50,
                        StartDate = DateTime.UtcNow,
                        EndDate = DateTime.UtcNow.AddMonths(2),
                        IsActive = true,
                        IsPublic = true,
                        CreatedAt = DateTime.UtcNow
                    }
                };

                context.Promotions.AddRange(promotions);
                await context.SaveChangesAsync();
            }
        }
    }
}
