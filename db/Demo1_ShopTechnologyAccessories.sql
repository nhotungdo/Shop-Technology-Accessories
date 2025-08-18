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
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- ===============================
-- 3. Bảng Users (Người dùng)
-- ===============================
CREATE TABLE Users (
    UserId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(255) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(255) NOT NULL,
    PhoneNumber NVARCHAR(20) NULL,
    RoleId INT NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Users_Roles FOREIGN KEY (RoleId) REFERENCES Roles(RoleId)
);
GO

-- ===============================
-- 4. Bảng Categories (Danh mục sản phẩm)
-- ===============================
CREATE TABLE Categories (
    CategoryId INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(100) NOT NULL UNIQUE,
    Description NVARCHAR(MAX) NULL
);
GO

-- ===============================
-- 5. Bảng Products (Sản phẩm)
-- ===============================
CREATE TABLE Products (
    ProductId INT PRIMARY KEY IDENTITY(1,1),
    CategoryId INT NOT NULL,
    ProductName NVARCHAR(255) NOT NULL,
    Description NVARCHAR(MAX) NULL,
    Price DECIMAL(18,2) NOT NULL,
    StockQuantity INT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL,
    CONSTRAINT FK_Products_Categories FOREIGN KEY (CategoryId) REFERENCES Categories(CategoryId)
);
GO

-- ===============================
-- 6. Bảng ProductImages (Ảnh sản phẩm)
-- ===============================
CREATE TABLE ProductImages (
    ImageId INT PRIMARY KEY IDENTITY(1,1),
    ProductId INT NOT NULL,
    ImageUrl NVARCHAR(500) NOT NULL,
    IsMain BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_ProductImages_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId) ON DELETE CASCADE
);
GO

-- ===============================
-- 7. Bảng Carts (Giỏ hàng)
-- ===============================
CREATE TABLE Carts (
    CartId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_Carts_Users FOREIGN KEY (UserId) REFERENCES Users(UserId) ON DELETE CASCADE
);
GO

-- ===============================
-- 8. Bảng CartItems (Chi tiết giỏ hàng)
-- ===============================
CREATE TABLE CartItems (
    CartItemId INT PRIMARY KEY IDENTITY(1,1),
    CartId UNIQUEIDENTIFIER NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL DEFAULT 1,
    CONSTRAINT FK_CartItems_Carts FOREIGN KEY (CartId) REFERENCES Carts(CartId) ON DELETE CASCADE,
    CONSTRAINT FK_CartItems_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
GO

-- ===============================
-- 9. Bảng Payments (Thanh toán)
-- ===============================
CREATE TABLE Payments (
    PaymentId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    Method NVARCHAR(50) NOT NULL, -- PayPal, VNPay, COD
    Amount DECIMAL(18,2) NOT NULL,
    PaymentDate DATETIME NOT NULL DEFAULT GETDATE(),
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending'
);
GO

-- ===============================
-- 10. Bảng Orders (Đơn hàng)
-- ===============================
CREATE TABLE Orders (
    OrderId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    UserId UNIQUEIDENTIFIER NOT NULL,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL,
    Status NVARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Paid, Shipped, Completed, Canceled
    PaymentId UNIQUEIDENTIFIER NULL,
    ShippingAddress NVARCHAR(255) NOT NULL,
    CONSTRAINT FK_Orders_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT FK_Orders_Payments FOREIGN KEY (PaymentId) REFERENCES Payments(PaymentId)
);
GO

-- ===============================
-- 11. Bảng OrderDetails (Chi tiết đơn hàng)
-- ===============================
CREATE TABLE OrderDetails (
    OrderDetailId INT PRIMARY KEY IDENTITY(1,1),
    OrderId UNIQUEIDENTIFIER NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    CONSTRAINT FK_OrderDetails_Orders FOREIGN KEY (OrderId) REFERENCES Orders(OrderId) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetails_Products FOREIGN KEY (ProductId) REFERENCES Products(ProductId)
);
GO

CREATE TABLE [Wishlists] (
    [WishlistId] int NOT NULL IDENTITY,
    [UserId] uniqueidentifier NOT NULL,
    [ProductId] int NOT NULL,
    CONSTRAINT [PK_Wishlists] PRIMARY KEY ([WishlistId]),
    CONSTRAINT [FK_Wishlists_Products] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([ProductId]),
    CONSTRAINT [FK_Wishlists_Users] FOREIGN KEY ([UserId]) REFERENCES [Users] ([UserId])
);
GO
CREATE INDEX [IX_Wishlists_ProductId] ON [Wishlists] ([ProductId]);
GO
CREATE INDEX [IX_Wishlists_UserId] ON [Wishlists] ([UserId]);
GO

-- ===============================
-- 12. Insert dữ liệu mẫu
-- ===============================
INSERT INTO Roles (RoleName) VALUES ('Admin'), ('User');

INSERT INTO Categories (CategoryName, Description) VALUES
(N'Sạc', N'Sạc điện thoại, laptop, tablet'),
(N'Tai nghe', N'Tai nghe có dây, không dây'),
(N'Ốp lưng', N'Ốp bảo vệ cho điện thoại, tablet'),
(N'Bàn phím', N'Bàn phím cơ, bàn phím không dây'),
(N'Chuột', N'Chuột gaming, chuột văn phòng');


INSERT INTO Users (
    FullName, Email, PasswordHash, PhoneNumber, RoleId, CreatedAt
) 
VALUES (
    N'Admin', 
    N'donhotung2004@gmail.com', 
    N'123456', 
    N'0931982568', 
    1, 
    GETDATE()
);

-- Enable IDENTITY_INSERT
SET IDENTITY_INSERT Products ON;
-- Chèn dữ liệu mẫu vào bảng Products cho cửa hàng phụ kiện công nghệ
INSERT INTO Products (CategoryId,ProductId ,ProductName, Description, Price, StockQuantity, CreatedAt)
VALUES 
(1, 1,N'Bộ chuyển đổi USB-C', N'Bộ chuyển đổi USB-C đa cổng với HDMI, USB 3.0 và Ethernet', 49.99, 150, GETDATE()),
(1, 2,N'Đế sạc không dây', N'Đế sạc không dây tốc độ cao tương thích với các thiết bị hỗ trợ Qi', 29.99, 200, GETDATE()),
(4, 3,N'Bàn phím Bluetooth', N'Bàn phím không dây nhỏ gọn với đèn nền', 59.99, 100, GETDATE()),
(1, 4,N'Ổ SSD di động 1TB', N'Ổ SSD ngoài tốc độ cao với giao diện USB-C', 129.99, 80, GETDATE()),
(2, 5,N'Tai nghe khử tiếng ồn', N'Tai nghe không dây với công nghệ khử tiếng ồn chủ động', 79.99, 120, GETDATE()),
(1, 6,N'Sạc nhanh USB-C 65W', N'Sạc nhanh cho laptop và điện thoại, hỗ trợ Power Delivery', 39.99, 180, GETDATE()),
(1, 7,N'Cáp sạc USB-C sang Lightning', N'Cáp sạc chất lượng cao cho iPhone và iPad', 19.99, 250, GETDATE()),
(2, 8,N'Tai nghe true wireless', N'Tai nghe không dây hoàn toàn với hộp sạc', 69.99, 150, GETDATE()),
(2, 9,N'Tai nghe có dây 3.5mm', N'Tai nghe có dây chất lượng âm thanh cao', 24.99, 200, GETDATE()),
(3, 10,N'Ốp lưng iPhone 14', N'Ốp lưng silicon bảo vệ chống sốc cho iPhone 14', 15.99, 300, GETDATE()),
(3, 11,N'Ốp lưng Samsung Galaxy S23', N'Ốp lưng trong suốt chống trầy xước', 12.99, 280, GETDATE()),
(4, 12,N'Bàn phím cơ RGB', N'Bàn phím cơ với đèn RGB tùy chỉnh', 89.99, 90, GETDATE()),
(4, 13,N'Bàn phím không dây mini', N'Bàn phím không dây siêu mỏng cho văn phòng', 49.99, 110, GETDATE()),
(5, 14,N'Chuột gaming không dây', N'Chuột gaming với cảm biến độ chính xác cao', 59.99, 130, GETDATE()),
(5, 15,N'Chuột văn phòng có dây', N'Chuột quang cơ bản cho công việc văn phòng', 9.99, 400, GETDATE());

INSERT INTO ProductImages (ProductId, ImageUrl, IsMain)
VALUES 
(1, N'https://viethansecurity.com/media/product/9507_bo_chuyen_doi_ugreen_40873_cm179.jpg', 1),
(1, N'https://www.tnc.com.vn/uploads/product/duyen2021/cable-usb-c-ugreen-40873.jpg', 0),
(2, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcRoaacgNWVoXN3W-mMdzeDdt7HZ6_QbiiDgqLgLcJYEwzRRipOE5qIyNyvgo5CxgjvqZgI&usqp=CAU', 1),
(2, N'https://product.hstatic.net/1000153276/product/tram_sac_khong_day_anker_maggo_-_a2557__3_trong_1_co_the_gap_lai__70124cb86d264f1ca0e0554a7dda1705_master.png', 0),
(3, N'https://cohotech.vn/wp-content/uploads/2025/05/NuPhy-Air75-V2-va-NuPhy-Air96-V2-2.webp', 1),
(3, N'https://cdn.tgdd.vn/Files/2022/07/17/1448545/ban-phim-bluetooth-la-gi-co-nen-mua-ban-phim-blue-6.jpg', 0),
(4, N'https://down-th.img.susercontent.com/file/th-11134208-7rasc-manr8zzg1vwf28', 1),
(4, N'https://thenhominhhang.com/thumbs/540x540x2/upload/product/portable-sandisk-1tb-e61-2-3542.jpg', 0),
(5, N'https://tainghe.com.vn/media/news/697_noisecancellingheadphones_1280_1519236823944_1280w.jpg', 1),
(5, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTyWk-m90mqwo4zIndchzmUhv8ttHyGmFn7pw&s', 0),
(6, N'https://bizweb.dktcdn.net/thumb/grande/100/031/560/products/broshop-gian-hang-chinh-hang-13.png?v=1709517970873', 1),
(6, N'https://ugreen.vn/wp-content/uploads/2022/11/cu-sac-nhanh-90495.jpg', 0),
(7, N'https://store.storeimages.cdn-apple.com/1/as-images.apple.com/is/MM0A3?wid=1144&hei=1144&fmt=jpeg&qlt=90&.v=M1QzMm9ybmlkQ3d6ZGgvOEtLT2s2d2tuVHYzMERCZURia3c5SzJFOTlPakdhd3hTd0Z4eVU5dlRoTFZsS1dHQnF1RHVsWWtITU8zLy9oRVFmWitIakE', 1),
(7, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTs6ncV9sFKHLmzERjJkXFcynnGswyezDZR1A&s', 0),
(8, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcSZXwPjZVe9AXHaBYGoJzbdXKhVs_hNIjwCOQ&s', 1),
(8, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcS2gRP7lHTnXn2pJHhYFoslpwrsuWvmfjCb4oreymxkE5_N5o7psk-AZ797HYZ3tyU_YkI&usqp=CAU', 0),
(9, N'https://product.hstatic.net/1000152881/product/c4dd061b-25e0-47a7-b659-4f32c33740d7_9429de8f9f344a08bf9d4ec7029b4d2f.jpg', 1),
(9, N'https://cdn2.cellphones.com.vn/insecure/rs:fill:0:358/q:90/plain/https://cellphones.com.vn/media/catalog/product/t/a/tai-nghe-co-day-robot-re101s-3-5mm_1_.png', 0),
(10, N'https://bizweb.dktcdn.net/100/031/560/products/broshop-op-lung-iphone-14-pro-max-spigen-liquid-crystal-clear-1-30f70b2b-3f1d-432d-8da4-6a71d98bcd33.png?v=1662808498227', 1),
(10, N'https://cdn1.viettelstore.vn/Images/Product/ProductImage/1891089918.jpeg', 0),
(11, N'https://lesang.vn/images/san-pham/op-lung-samsung-galaxy-s23-plus-spigen-liquid-air1675510298.jpg', 1),
(11, N'https://ringkevietnam.com/wp-content/uploads/2023/01/op-lung-samsung-galaxy-s23-ringke-fusion-ringkevietnam-04.jpg', 0),
(12, N'https://linhkienstore.vn/plugins/responsive_filemanager/source/Ngoc%20Anh/Attack%20Shark%20K75%20RGB/K75-RGB-so-huu-thiet-ke-trong-suot-co-the-xuyen-led-Ban-phim-co-co-day-Attack-Shark-K75-RGB-trong-suot.jpg', 1),
(12, N'https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcTrEy4MzdaciYJHMtzdAKhe_x_md2bDd6HyEg&s', 0),
(13, N'https://macinsta.vn/wp-content/uploads/2023/07/MI54-12.jpg', 1),
(13, N'https://fastcomputer.com.vn/wp-content/uploads/2020/06/bo-chuot-va-ban-phim-mini-wireless-gkm901-9-1.jpg', 0),
(14, N'https://dareu.com.vn/wp-content/uploads/2021/11/chuot-khong-day-gaming-dareu-em901x-01.jpg', 1),
(14, N'https://www.sieuthimaychu.vn/datafiles/setone/15853625509855.jpg', 0),
(15, N'https://bizweb.dktcdn.net/100/505/802/products/23.png?v=1731603026967', 1),
(15, N'https://t-wolf.vn/wp-content/uploads/2024/05/chuot-khong-day-twolf-g580-nhay.jpg', 0);





UPDATE Products
SET CategoryId = 2
WHERE ProductName = 'Ổ SSD di động 1TB' 
  AND CategoryId = 1;

