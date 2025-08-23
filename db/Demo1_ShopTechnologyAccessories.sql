-- ===============================
-- 1. Tạo Database
-- ===============================
CREATE DATABASE ShopTechnologyAccessories;
GO

USE ShopTechnologyAccessories;
GO

-- ===============================
-- 2. Bảng Roles (Vai trò người dùng)
-- ===============================
CREATE TABLE Roles (
    RoleId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(50) NOT NULL UNIQUE,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE()
);
GO

-- ===============================
-- 3. Bảng Users (Người dùng)
-- ===============================
CREATE TABLE Users (
    UserId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PhoneNumber NVARCHAR(20) NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Address NVARCHAR(255) NULL,
    City NVARCHAR(100) NULL,
    Province NVARCHAR(100) NULL,
    PostalCode NVARCHAR(20) NULL,
    DateOfBirth DATETIME NOT NULL,
    Avatar NVARCHAR(255) NULL,
    IsEmailVerified BIT NOT NULL DEFAULT 0,
    IsPhoneVerified BIT NOT NULL DEFAULT 0,
    EmailVerificationToken NVARCHAR(255) NULL,
    EmailVerificationExpiry DATETIME NULL,
    PasswordResetToken NVARCHAR(255) NULL,
    PasswordResetExpiry DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    SocialLoginProvider NVARCHAR(50) NULL,
    SocialLoginId NVARCHAR(255) NULL
);
GO

-- ===============================
-- 4. Bảng UserRoles (Quan hệ nhiều-nhiều giữa User và Role)
-- ===============================
CREATE TABLE UserRoles (
    UserRoleId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    RoleId INT NOT NULL,
    AssignedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_UserRoles_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_UserRoles_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
GO

-- ===============================
-- 5. Bảng Categories (Danh mục sản phẩm)
-- ===============================
CREATE TABLE Categories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    ImageUrl NVARCHAR(255) NULL,
    ParentCategoryId INT NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsFeatured BIT NOT NULL DEFAULT 0,
    Slug NVARCHAR(100) NULL UNIQUE,
    MetaTitle NVARCHAR(255) NULL,
    MetaDescription NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Categories_ParentCategory FOREIGN KEY (ParentCategoryId) REFERENCES Categories(CategoryId)
);
GO

-- ===============================
-- 6. Bảng Products (Sản phẩm)
-- ===============================
CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000) NULL,
    Price DECIMAL(18,2) NOT NULL,
    OriginalPrice DECIMAL(18,2) NULL,
    Brand NVARCHAR(100) NULL,
    Model NVARCHAR(100) NULL,
    SKU NVARCHAR(50) NULL UNIQUE,
    StockQuantity INT NOT NULL DEFAULT 0,
    CategoryId INT NOT NULL,
    MainImage NVARCHAR(255) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsFeatured BIT NOT NULL DEFAULT 0,
    IsNew BIT NOT NULL DEFAULT 0,
    IsHot BIT NOT NULL DEFAULT 0,
    ViewCount INT NOT NULL DEFAULT 0,
    SoldCount INT NOT NULL DEFAULT 0,
    AverageRating DECIMAL(3,2) NULL,
    ReviewCount INT NOT NULL DEFAULT 0,
    Slug NVARCHAR(100) NULL UNIQUE,
    MetaTitle NVARCHAR(255) NULL,
    MetaDescription NVARCHAR(500) NULL,
    Keywords NVARCHAR(500) NULL,
    Color NVARCHAR(100) NULL,
    Material NVARCHAR(100) NULL,
    Weight NVARCHAR(100) NULL,
    Dimensions NVARCHAR(100) NULL,
    Compatibility NVARCHAR(500) NULL,
    Warranty NVARCHAR(100) NULL,
    Features NVARCHAR(500) NULL,
    PackageContents NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);
GO

-- ===============================
-- 7. Bảng ProductImages (Ảnh sản phẩm)
-- ===============================
CREATE TABLE ProductImages (
    ProductImageId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(255) NOT NULL,
    IsMain BIT NOT NULL DEFAULT 0,
    DisplayOrder INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
);
GO

-- ===============================
-- 8. Bảng Reviews (Đánh giá sản phẩm)
-- ===============================
CREATE TABLE Reviews (
    ReviewId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
    Title NVARCHAR(255) NULL,
    Comment NVARCHAR(1000) NULL,
    IsVerified BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Reviews_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Reviews_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId),
    CONSTRAINT UQ_User_Product_Review UNIQUE (UserId, ProductId)
);
GO

-- ===============================
-- 9. Bảng ReviewImages (Ảnh đánh giá)
-- ===============================
CREATE TABLE ReviewImages (
    ReviewImageId INT PRIMARY KEY IDENTITY(1,1),
    ReviewId INT NOT NULL,
    ImageUrl NVARCHAR(255) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_ReviewImages_Reviews FOREIGN KEY (ReviewId) REFERENCES Reviews(ReviewId) ON DELETE CASCADE
);
GO

-- ===============================
-- 10. Bảng Carts (Giỏ hàng)
-- ===============================
CREATE TABLE Carts (
    CartId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    SessionId NVARCHAR(100) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Carts_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

-- ===============================
-- 11. Bảng CartItems (Chi tiết giỏ hàng)
-- ===============================
CREATE TABLE CartItems (
    CartItemId INT PRIMARY KEY IDENTITY(1,1),
    CartId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES Carts(CartId) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
GO

-- ===============================
-- 12. Bảng Orders (Đơn hàng)
-- ===============================
CREATE TABLE Orders (
    OrderId INT PRIMARY KEY IDENTITY(1,1),
    OrderNumber NVARCHAR(50) NOT NULL UNIQUE,
    UserId INT NOT NULL,
    CustomerName NVARCHAR(100) NOT NULL,
    CustomerEmail NVARCHAR(150) NOT NULL,
    CustomerPhone NVARCHAR(20) NOT NULL,
    ShippingAddress NVARCHAR(255) NOT NULL,
    ShippingCity NVARCHAR(100) NULL,
    ShippingProvince NVARCHAR(100) NULL,
    ShippingPostalCode NVARCHAR(20) NULL,
    OrderNotes NVARCHAR(500) NULL,
    SubTotal DECIMAL(18,2) NOT NULL,
    TaxAmount DECIMAL(18,2) NOT NULL,
    ShippingFee DECIMAL(18,2) NOT NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL,
    OrderStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    PaymentStatus NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    PaymentMethod NVARCHAR(50) NULL,
    TrackingNumber NVARCHAR(100) NULL,
    ShippingMethod NVARCHAR(100) NULL,
    EstimatedDeliveryDate DATETIME NULL,
    ShippedDate DATETIME NULL,
    DeliveredDate DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId)
);
GO

-- ===============================
-- 13. Bảng OrderDetails (Chi tiết đơn hàng)
-- ===============================
CREATE TABLE OrderDetails (
    OrderDetailId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    ProductId INT NOT NULL,
    ProductName NVARCHAR(200) NOT NULL,
    ProductSKU NVARCHAR(100) NULL,
    Quantity INT NOT NULL DEFAULT 1,
    UnitPrice DECIMAL(18,2) NOT NULL,
    TotalPrice DECIMAL(18,2) NOT NULL,
    ProductImage NVARCHAR(255) NULL,
    ProductBrand NVARCHAR(100) NULL,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
GO

-- ===============================
-- 14. Bảng OrderHistory (Lịch sử đơn hàng)
-- ===============================
CREATE TABLE OrderHistory (
    OrderHistoryId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    Status NVARCHAR(50) NOT NULL,
    Notes NVARCHAR(500) NULL,
    UpdatedByUserId INT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_OrderHistory_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderHistory_Users FOREIGN KEY (UpdatedByUserId) REFERENCES Users(UserId)
);
GO

-- ===============================
-- 15. Bảng Payments (Thanh toán)
-- ===============================
CREATE TABLE Payments (
    PaymentId INT PRIMARY KEY IDENTITY(1,1),
    OrderId INT NOT NULL,
    PaymentMethod NVARCHAR(50) NOT NULL,
    PaymentProvider NVARCHAR(100) NULL,
    TransactionId NVARCHAR(100) NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending',
    Amount DECIMAL(18,2) NOT NULL,
    Description NVARCHAR(500) NULL,
    ErrorMessage NVARCHAR(500) NULL,
    PaymentUrl NVARCHAR(255) NULL,
    CallbackData NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Payments_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId)
);
GO

-- ===============================
-- 16. Bảng Promotions (Khuyến mãi)
-- ===============================
CREATE TABLE Promotions (
    PromotionId INT PRIMARY KEY IDENTITY(1,1),
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    Code NVARCHAR(50) NULL UNIQUE,
    DiscountType NVARCHAR(20) NOT NULL DEFAULT 'Percentage',
    DiscountValue DECIMAL(18,2) NOT NULL,
    MinimumOrderAmount DECIMAL(18,2) NULL,
    MaximumDiscountAmount DECIMAL(18,2) NULL,
    UsageLimit INT NULL,
    UsedCount INT NOT NULL DEFAULT 0,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    IsPublic BIT NOT NULL DEFAULT 1,
    ImageUrl NVARCHAR(255) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);
GO

-- ===============================
-- 17. Bảng ProductPromotions (Quan hệ nhiều-nhiều giữa Product và Promotion)
-- ===============================
CREATE TABLE ProductPromotions (
    ProductPromotionId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    PromotionId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_ProductPromotions_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
    CONSTRAINT FK_ProductPromotions_Promotions FOREIGN KEY (PromotionId) REFERENCES Promotions(PromotionId) ON DELETE CASCADE
);
GO

-- ===============================
-- 18. Bảng Banners (Banner quảng cáo)
-- ===============================
CREATE TABLE Banners (
    BannerId INT PRIMARY KEY IDENTITY(1,1),
    Title NVARCHAR(100) NOT NULL,
    ImageUrl NVARCHAR(255) NOT NULL,
    LinkUrl NVARCHAR(255) NULL,
    Position NVARCHAR(50) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    StartDate DATETIME NULL,
    EndDate DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);
GO

-- ===============================
-- 19. Bảng Wishlists (Danh sách yêu thích)
-- ===============================
CREATE TABLE Wishlists (
    WishlistId INT PRIMARY KEY IDENTITY(1,1),
    UserId INT NOT NULL,
    ProductId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Wishlists_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE,
    CONSTRAINT FK_Wishlists_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE,
    CONSTRAINT UQ_User_Product_Wishlist UNIQUE (UserId, ProductId)
);
GO

-- ===============================
-- 20. Bảng Contacts (Liên hệ)
-- ===============================
CREATE TABLE Contacts (
    ContactId INT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    Subject NVARCHAR(200) NOT NULL,
    Message NVARCHAR(1000) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'New',
    ReplyMessage NVARCHAR(1000) NULL,
    RepliedByUserId INT NULL,
    RepliedAt DATETIME NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Contacts_Users FOREIGN KEY (RepliedByUserId) REFERENCES Users(UserId)
);
GO

-- ===============================
-- 21. Bảng FAQs (Câu hỏi thường gặp)
-- ===============================
CREATE TABLE FAQs (
    FAQId INT PRIMARY KEY IDENTITY(1,1),
    Question NVARCHAR(200) NOT NULL,
    Answer NVARCHAR(2000) NOT NULL,
    Category NVARCHAR(50) NULL,
    DisplayOrder INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);
GO

-- ===============================
-- 22. Tạo Index để tối ưu hiệu suất
-- ===============================
-- Users
CREATE INDEX IX_Users_Email ON Users(Email);
CREATE INDEX IX_Users_PhoneNumber ON Users(PhoneNumber);
CREATE INDEX IX_Users_IsActive ON Users(IsActive);

-- Products
CREATE INDEX IX_Products_CategoryId ON Products(CategoryId);
CREATE INDEX IX_Products_SKU ON Products(SKU);
CREATE INDEX IX_Products_Slug ON Products(Slug);
CREATE INDEX IX_Products_IsActive ON Products(IsActive);
CREATE INDEX IX_Products_IsFeatured ON Products(IsFeatured);
CREATE INDEX IX_Products_Price ON Products(Price);

-- Categories
CREATE INDEX IX_Categories_ParentCategoryId ON Categories(ParentCategoryId);
CREATE INDEX IX_Categories_Slug ON Categories(Slug);
CREATE INDEX IX_Categories_IsActive ON Categories(IsActive);

-- Orders
CREATE INDEX IX_Orders_UserId ON Orders(UserId);
CREATE INDEX IX_Orders_OrderNumber ON Orders(OrderNumber);
CREATE INDEX IX_Orders_OrderStatus ON Orders(OrderStatus);
CREATE INDEX IX_Orders_PaymentStatus ON Orders(PaymentStatus);
CREATE INDEX IX_Orders_CreatedAt ON Orders(CreatedAt);

-- Reviews
CREATE INDEX IX_Reviews_ProductId ON Reviews(ProductId);
CREATE INDEX IX_Reviews_UserId ON Reviews(UserId);
CREATE INDEX IX_Reviews_Rating ON Reviews(Rating);

-- Promotions
CREATE INDEX IX_Promotions_Code ON Promotions(Code);
CREATE INDEX IX_Promotions_IsActive ON Promotions(IsActive);
CREATE INDEX IX_Promotions_StartDate ON Promotions(StartDate);
CREATE INDEX IX_Promotions_EndDate ON Promotions(EndDate);

-- Carts
CREATE INDEX IX_Carts_UserId ON Carts(UserId);
CREATE INDEX IX_Carts_SessionId ON Carts(SessionId);

-- Wishlists
CREATE INDEX IX_Wishlists_UserId ON Wishlists(UserId);
CREATE INDEX IX_Wishlists_ProductId ON Wishlists(ProductId);

-- Contacts
CREATE INDEX IX_Contacts_Email ON Contacts(Email);
CREATE INDEX IX_Contacts_Status ON Contacts(Status);

-- Banners
CREATE INDEX IX_Banners_Position ON Banners(Position);
CREATE INDEX IX_Banners_IsActive ON Banners(IsActive);
GO

-- ===============================
-- 23. Insert dữ liệu mẫu
-- ===============================

-- Roles
INSERT INTO Roles (Name) VALUES ('Admin'), ('User');

-- Categories
INSERT INTO Categories (Name, Description, Slug, IsActive, IsFeatured) VALUES
(N'Sạc', N'Sạc điện thoại, laptop, tablet', N'sac', 1, 1),
(N'Tai nghe', N'Tai nghe có dây, không dây', N'tai-nghe', 1, 1),
(N'Ốp lưng', N'Ốp bảo vệ cho điện thoại, tablet', N'op-lung', 1, 0),
(N'Bàn phím', N'Bàn phím cơ, bàn phím không dây', N'ban-phim', 1, 1),
(N'Chuột', N'Chuột gaming, chuột văn phòng', N'chuot', 1, 0);

-- Users
INSERT INTO Users (FullName, Email, PhoneNumber, Password, DateOfBirth, IsEmailVerified, IsActive) VALUES
(N'Admin', N'donhotung2004@gmail.com', N'0931982568', N'123456', '1990-01-01', 1, 1),
(N'Nguyễn Văn A', N'nguyenvana@gmail.com', N'0123456789', N'123456', '1995-05-15', 1, 1),
(N'Trần Thị B', N'tranthib@gmail.com', N'0987654321', N'123456', '1992-08-20', 1, 1);

-- UserRoles
INSERT INTO UserRoles (UserId, RoleId) VALUES
(1, 1), -- Admin
(2, 2), -- User
(3, 2); -- User

-- Products
INSERT INTO Products (Name, Description, Price, OriginalPrice, Brand, SKU, StockQuantity, CategoryId, IsActive, IsFeatured, Slug) VALUES
(N'Bộ chuyển đổi USB-C', N'Bộ chuyển đổi USB-C đa cổng với HDMI, USB 3.0 và Ethernet', 49.99, 59.99, N'UGreen', N'UG-USB-C-001', 150, 1, 1, 1, N'bo-chuyen-doi-usb-c'),
(N'Đế sạc không dây', N'Đế sạc không dây tốc độ cao tương thích với các thiết bị hỗ trợ Qi', 29.99, 39.99, N'Anker', N'ANK-WIRELESS-001', 200, 1, 1, 1, N'de-sac-khong-day'),
(N'Bàn phím Bluetooth', N'Bàn phím không dây nhỏ gọn với đèn nền', 59.99, 69.99, N'Logitech', N'LOG-BT-KB-001', 100, 4, 1, 1, N'ban-phim-bluetooth'),
(N'Ổ SSD di động 1TB', N'Ổ SSD ngoài tốc độ cao với giao diện USB-C', 129.99, 149.99, N'SanDisk', N'SAND-1TB-001', 80, 1, 1, 0, N'o-ssd-di-dong-1tb'),
(N'Tai nghe khử tiếng ồn', N'Tai nghe không dây với công nghệ khử tiếng ồn chủ động', 79.99, 99.99, N'Sony', N'SONY-NC-001', 120, 2, 1, 1, N'tai-nghe-khu-tieng-on'),
(N'Sạc nhanh USB-C 65W', N'Sạc nhanh cho laptop và điện thoại, hỗ trợ Power Delivery', 39.99, 49.99, N'UGreen', N'UG-65W-001', 180, 1, 1, 0, N'sac-nhanh-usb-c-65w'),
(N'Cáp sạc USB-C sang Lightning', N'Cáp sạc chất lượng cao cho iPhone và iPad', 19.99, 24.99, N'Apple', N'APP-CABLE-001', 250, 1, 1, 0, N'cap-sac-usb-c-lightning'),
(N'Tai nghe true wireless', N'Tai nghe không dây hoàn toàn với hộp sạc', 69.99, 89.99, N'Samsung', N'SAM-TW-001', 150, 2, 1, 1, N'tai-nghe-true-wireless'),
(N'Tai nghe có dây 3.5mm', N'Tai nghe có dây chất lượng âm thanh cao', 24.99, 29.99, N'Sennheiser', N'SEN-3.5-001', 200, 2, 1, 0, N'tai-nghe-co-day-3-5mm'),
(N'Ốp lưng iPhone 14', N'Ốp lưng silicon bảo vệ chống sốc cho iPhone 14', 15.99, 19.99, N'Spigen', N'SPI-IP14-001', 300, 3, 1, 0, N'op-lung-iphone-14'),
(N'Ốp lưng Samsung Galaxy S23', N'Ốp lưng trong suốt chống trầy xước', 12.99, 15.99, N'Ringke', N'RIN-S23-001', 280, 3, 1, 0, N'op-lung-samsung-s23'),
(N'Bàn phím cơ RGB', N'Bàn phím cơ với đèn RGB tùy chỉnh', 89.99, 109.99, N'Attack Shark', N'ATT-RGB-001', 90, 4, 1, 1, N'ban-phim-co-rgb'),
(N'Bàn phím không dây mini', N'Bàn phím không dây siêu mỏng cho văn phòng', 49.99, 59.99, N'Logitech', N'LOG-MINI-001', 110, 4, 1, 0, N'ban-phim-khong-day-mini'),
(N'Chuột gaming không dây', N'Chuột gaming với cảm biến độ chính xác cao', 59.99, 69.99, N'DareU', N'DAR-GAMING-001', 130, 5, 1, 1, N'chuot-gaming-khong-day'),
(N'Chuột văn phòng có dây', N'Chuột quang cơ bản cho công việc văn phòng', 9.99, 12.99, N'Logitech', N'LOG-OFFICE-001', 400, 5, 1, 0, N'chuot-van-phong-co-day');

-- ProductImages
INSERT INTO ProductImages (ProductId, ImageUrl, IsMain, DisplayOrder) VALUES
(1, N'https://viethansecurity.com/media/product/9507_bo_chuyen_doi_ugreen_40873_cm179.jpg', 1, 1),
(1, N'https://www.tnc.com.vn/uploads/product/duyen2021/cable-usb-c-ugreen-40873.jpg', 0, 2),
(2, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRoaacgNWVoXN3W-mMdzeDdt7HZ6_QbiiDgqLgLcJYEwzRRipOE5qIyNyvgo5CxgjvqZgI&usqp=CAU', 1, 1),
(2, N'https://product.hstatic.net/1000153276/product/tram_sac_khong_day_anker_maggo_-_a2557__3_trong_1_co_the_gap_lai__70124cb86d264f1ca0e0554a7dda1705_master.png', 0, 2),
(3, N'https://cohotech.vn/wp-content/uploads/2025/05/NuPhy-Air75-V2-va-NuPhy-Air96-V2-2.webp', 1, 1),
(3, N'https://cdn.tgdd.vn/Files/2022/07/17/1448545/ban-phim-bluetooth-la-gi-co-nen-mua-ban-phim-blue-6.jpg', 0, 2),
(4, N'https://down-th.img.susercontent.com/file/th-11134208-7rasc-manr8zzg1vwf28', 1, 1),
(4, N'https://thenhominhhang.com/thumbs/540x540x2/upload/product/portable-sandisk-1tb-e61-2-3542.jpg', 0, 2),
(5, N'https://tainghe.com.vn/media/news/697_noisecancellingheadphones_1280_1519236823944_1280w.jpg', 1, 1),
(5, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTyWk-m90mqwo4zIndchzmUhv8ttHyGmFn7pw&s', 0, 2),
(6, N'https://bizweb.dktcdn.net/thumb/grande/100/031/560/products/broshop-gian-hang-chinh-hang-13.png?v=1709517970873', 1, 1),
(6, N'https://ugreen.vn/wp-content/uploads/2022/11/cu-sac-nhanh-90495.jpg', 0, 2),
(7, N'https://store.storeimages.cdn-apple.com/1/as-images.apple.com/is/MM0A3?wid=1144&hei=1144&fmt=jpeg&qlt=90&.v=M1QzMm9ybmlkQ3d6ZGgvOEtLT2s2d2tuVHYzMERCZURia3c5SzJFOTlPakdhd3hTd0Z4eVU5dlRoTFZsS1dHQnF1RHVsWWtITU8zLy9oRVFmWitIakE', 1, 1),
(7, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTs6ncV9sFKHLmzERjJkXFcynnGswyezDZR1A&s', 0, 2),
(8, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSZXwPjZVe9AXHaBYGoJzbdXKhVs_hNIjwCOQ&s', 1, 1),
(8, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS2gRP7lHTnXn2pJHhYFoslpwrsuWvmfjCb4oreymxkE5_N5o7psk-AZ797HYZ3tyU_YkI&usqp=CAU', 0, 2),
(9, N'https://product.hstatic.net/1000152881/product/c4dd061b-25e0-47a7-b659-4f32c33740d7_9429de8f9f344a08bf9d4ec7029b4d2f.jpg', 1, 1),
(9, N'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/a/tai-nghe-co-day-robot-re101s-3-5mm_1_.png', 0, 2),
(10, N'https://bizweb.dktcdn.net/100/031/560/products/broshop-op-lung-iphone-14-pro-max-spigen-liquid-crystal-clear-1-30f70b2b-3f1d-432d-8da4-6a71d98bcd33.png?v=1662808498227', 1, 1),
(10, N'https://cdn1.viettelstore.vn/Images/Product/ProductImage/1891089918.jpeg', 0, 2),
(11, N'https://lesang.vn/images/san-pham/op-lung-samsung-galaxy-s23-plus-spigen-liquid-air1675510298.jpg', 1, 1),
(11, N'https://ringkevietnam.com/wp-content/uploads/2023/01/op-lung-samsung-galaxy-s23-ringke-fusion-ringkevietnam-04.jpg', 0, 2),
(12, N'https://linhkienstore.vn/plugins/responsive_filemanager/source/Ngoc%20Anh/Attack%20Shark%20K75%20RGB/K75-RGB-so-huu-thiet-ke-trong-suot-co-the-xuyen-led-Ban-phim-co-co-day-Attack-Shark-K75-RGB-trong-suot.jpg', 1, 1),
(12, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTrEy4MzdaciYJHMtzdAKhe_x_md2bDd6HyEg&s', 0, 2),
(13, N'https://macinsta.vn/wp-content/uploads/2023/07/MI54-12.jpg', 1, 1),
(13, N'https://fastcomputer.com.vn/wp-content/uploads/2020/06/bo-chuot-va-ban-phim-mini-wireless-gkm901-9-1.jpg', 0, 2),
(14, N'https://dareu.com.vn/wp-content/uploads/2021/11/chuot-khong-day-gaming-dareu-em901x-01.jpg', 1, 1),
(14, N'https://www.sieuthimaychu.vn/datafiles/setone/15853625509855.jpg', 0, 2),
(15, N'https://bizweb.dktcdn.net/100/505/802/products/23.png?v=1731603026967', 1, 1),
(15, N'https://t-wolf.vn/wp-content/uploads/2024/05/chuot-khong-day-twolf-g580-nhay.jpg', 0, 2);

-- Promotions
INSERT INTO Promotions (Name, Description, Code, DiscountType, DiscountValue, MinimumOrderAmount, MaximumDiscountAmount, UsageLimit, StartDate, EndDate, IsActive, IsPublic) VALUES
(N'Chào mừng khách hàng mới', N'Giảm 10% cho đơn hàng đầu tiên', N'WELCOME10', N'Percentage', 10.00, 100000, 50000, 1, GETDATE(), DATEADD(MONTH, 6, GETDATE()), 1, 1),
(N'Tiết kiệm 20%', N'Giảm 20% cho đơn hàng từ 500k', N'SAVE20', N'Percentage', 20.00, 500000, 200000, 100, GETDATE(), DATEADD(MONTH, 3, GETDATE()), 1, 1),
(N'Miễn phí vận chuyển', N'Miễn phí vận chuyển cho đơn hàng từ 300k', N'FREESHIP', N'FixedAmount', 50000, 300000, 50000, 50, GETDATE(), DATEADD(MONTH, 2, GETDATE()), 1, 1),
(N'Flash Sale 50%', N'Giảm 50% cho các sản phẩm được chọn', N'FLASH50', N'Percentage', 50.00, 200000, 300000, 20, GETDATE(), DATEADD(DAY, 7, GETDATE()), 1, 1);

-- Banners
INSERT INTO Banners (Title, ImageUrl, LinkUrl, Position, DisplayOrder, IsActive) VALUES
(N'Khuyến mãi mùa hè', N'https://img.freepik.com/free-vector/special-offer-modern-sale-banner_1017-20667.jpg', N'/promotions', N'Home', 1, 1),
(N'Sản phẩm mới', N'https://img.freepik.com/free-vector/gradient-sale-background_23-2148934475.jpg', N'/products/new', N'Home', 2, 1),
(N'Bàn phím gaming', N'https://img.freepik.com/free-vector/realistic-sale-background_23-2148934476.jpg', N'/category/ban-phim', N'Category', 1, 1);

-- FAQs
INSERT INTO FAQs (Question, Answer, Category, DisplayOrder, IsActive) VALUES
(N'Làm thế nào để đặt hàng?', N'Bạn có thể đặt hàng bằng cách thêm sản phẩm vào giỏ hàng và tiến hành thanh toán.', N'Đặt hàng', 1, 1),
(N'Thời gian giao hàng là bao lâu?', N'Thời gian giao hàng từ 1-3 ngày làm việc tùy thuộc vào địa chỉ giao hàng.', N'Giao hàng', 2, 1),
(N'Có thể đổi trả sản phẩm không?', N'Có, bạn có thể đổi trả sản phẩm trong vòng 30 ngày kể từ ngày nhận hàng.', N'Đổi trả', 3, 1),
(N'Các phương thức thanh toán nào được chấp nhận?', N'Chúng tôi chấp nhận thanh toán bằng tiền mặt, chuyển khoản ngân hàng, và các ví điện tử.', N'Thanh toán', 4, 1);

-- Reviews
INSERT INTO Reviews (UserId, ProductId, Rating, Title, Comment, IsVerified) VALUES
(2, 1, 5, N'Sản phẩm chất lượng', N'Bộ chuyển đổi USB-C rất tiện lợi, chất lượng tốt.', 1),
(3, 1, 4, N'Tốt nhưng hơi đắt', N'Sản phẩm tốt nhưng giá hơi cao một chút.', 1),
(2, 3, 5, N'Bàn phím tuyệt vời', N'Bàn phím Bluetooth rất nhạy, pin trâu.', 1),
(3, 5, 4, N'Tai nghe tốt', N'Chất lượng âm thanh tốt, khử tiếng ồn hiệu quả.', 1);

-- Wishlists
INSERT INTO Wishlists (UserId, ProductId) VALUES
(2, 1),
(2, 3),
(3, 5),
(3, 8);

-- Contacts
INSERT INTO Contacts (FullName, Email, PhoneNumber, Subject, Message, Status) VALUES
(N'Nguyễn Văn C', N'nguyenvanc@gmail.com', N'0123456788', N'Hỏi về sản phẩm', N'Tôi muốn hỏi về thông tin sản phẩm bàn phím cơ.', N'New'),
(N'Trần Thị D', N'tranthid@gmail.com', N'0987654322', N'Khiếu nại', N'Sản phẩm tôi nhận không đúng như mô tả.', N'New');

GO

-- ===============================
-- 24. Tạo View để thống kê đánh giá
-- ===============================
CREATE VIEW ProductReviewSummary AS
SELECT 
    p.ProductId,
    p.Name AS ProductName,
    AVG(CAST(r.Rating AS FLOAT)) AS AverageRating,
    COUNT(r.ReviewId) AS TotalReviews,
    SUM(CASE WHEN r.Rating = 5 THEN 1 ELSE 0 END) AS FiveStarCount,
    SUM(CASE WHEN r.Rating = 4 THEN 1 ELSE 0 END) AS FourStarCount,
    SUM(CASE WHEN r.Rating = 3 THEN 1 ELSE 0 END) AS ThreeStarCount,
    SUM(CASE WHEN r.Rating = 2 THEN 1 ELSE 0 END) AS TwoStarCount,
    SUM(CASE WHEN r.Rating = 1 THEN 1 ELSE 0 END) AS OneStarCount
FROM Products p
LEFT JOIN Reviews r ON p.ProductId = r.ProductId
GROUP BY p.ProductId, p.Name;
GO

-- ===============================
-- 25. Tạo Stored Procedure để tính discount
-- ===============================
CREATE PROCEDURE CalculatePromotionDiscount
    @PromotionCode NVARCHAR(50),
    @OrderAmount DECIMAL(18,2),
    @DiscountAmount DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @PromotionId INT, @DiscountType NVARCHAR(20), @DiscountValue DECIMAL(18,2);
    DECLARE @MinAmount DECIMAL(18,2), @MaxDiscount DECIMAL(18,2);
    DECLARE @IsActive BIT, @UsedCount INT, @UsageLimit INT;
    DECLARE @StartDate DATETIME, @EndDate DATETIME;
    
    -- Lấy thông tin promotion
    SELECT 
        @PromotionId = PromotionId,
        @DiscountType = DiscountType,
        @DiscountValue = DiscountValue,
        @MinAmount = MinimumOrderAmount,
        @MaxDiscount = MaximumDiscountAmount,
        @IsActive = IsActive,
        @UsedCount = UsedCount,
        @UsageLimit = UsageLimit,
        @StartDate = StartDate,
        @EndDate = EndDate
    FROM Promotions 
    WHERE Code = @PromotionCode;
    
    -- Kiểm tra promotion có hợp lệ không
    IF @PromotionId IS NULL OR 
       @IsActive = 0 OR 
       (@UsageLimit IS NOT NULL AND @UsedCount >= @UsageLimit) OR
       GETDATE() < @StartDate OR 
       GETDATE() > @EndDate OR
       (@MinAmount IS NOT NULL AND @OrderAmount < @MinAmount)
    BEGIN
        SET @DiscountAmount = 0;
        RETURN;
    END
    
    -- Tính discount
    IF @DiscountType = 'Percentage'
        SET @DiscountAmount = @OrderAmount * (@DiscountValue / 100);
    ELSE
        SET @DiscountAmount = @DiscountValue;
    
    -- Đảm bảo discount không vượt quá giới hạn
    IF @MaxDiscount IS NOT NULL AND @DiscountAmount > @MaxDiscount
        SET @DiscountAmount = @MaxDiscount;
    
    -- Đảm bảo discount không vượt quá order amount
    IF @DiscountAmount > @OrderAmount
        SET @DiscountAmount = @OrderAmount;
END
GO

-- ===============================
-- 26. Tạo Stored Procedure để cleanup expired tokens
-- ===============================
CREATE PROCEDURE CleanupExpiredPasswordResets
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Users 
    SET PasswordResetToken = NULL, 
        PasswordResetExpiry = NULL 
    WHERE PasswordResetExpiry < GETDATE();
    
    PRINT 'Đã xóa các token hết hạn';
END
GO

-- ===============================
-- 27. Tạo Stored Procedure để cập nhật rating sản phẩm
-- ===============================
CREATE PROCEDURE UpdateProductRating
    @ProductId INT
AS
BEGIN
    SET NOCOUNT ON;
    
    UPDATE Products 
    SET AverageRating = (
        SELECT AVG(CAST(Rating AS DECIMAL(3,2)))
        FROM Reviews 
        WHERE ProductId = @ProductId
    ),
    ReviewCount = (
        SELECT COUNT(*)
        FROM Reviews 
        WHERE ProductId = @ProductId
    )
    WHERE ProductId = @ProductId;
END
GO

PRINT 'Database ShopTechnologyAccessories đã được tạo thành công!';
PRINT 'Tổng cộng: 21 bảng, 25+ index, 3 stored procedures, 1 view';
PRINT 'Dữ liệu mẫu đã được thêm vào các bảng chính';
