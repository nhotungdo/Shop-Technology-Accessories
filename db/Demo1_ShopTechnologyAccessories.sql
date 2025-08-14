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

-- Chèn dữ liệu mẫu vào bảng Products cho cửa hàng phụ kiện công nghệ
INSERT INTO Products (CategoryId, ProductName, Description, Price, StockQuantity, CreatedAt)
VALUES 
(1, N'Bộ chuyển đổi USB-C', N'Bộ chuyển đổi USB-C đa cổng với HDMI, USB 3.0 và Ethernet', 49.99, 150, GETDATE()),
(1, N'Đế sạc không dây', N'Đế sạc không dây tốc độ cao tương thích với các thiết bị hỗ trợ Qi', 29.99, 200, GETDATE()),
(2, N'Bàn phím Bluetooth', N'Bàn phím không dây nhỏ gọn với đèn nền', 59.99, 100, GETDATE()),
(2, N'Ổ SSD di động 1TB', N'Ổ SSD ngoài tốc độ cao với giao diện USB-C', 129.99, 80, GETDATE()),
(3, N'Tai nghe khử tiếng ồn', N'Tai nghe không dây với công nghệ khử tiếng ồn chủ động', 79.99, 120, GETDATE()),
(1, N'Sạc nhanh USB-C 65W', N'Sạc nhanh cho laptop và điện thoại, hỗ trợ Power Delivery', 39.99, 180, GETDATE()),
(1, N'Cáp sạc USB-C sang Lightning', N'Cáp sạc chất lượng cao cho iPhone và iPad', 19.99, 250, GETDATE()),
(2, N'Tai nghe true wireless', N'Tai nghe không dây hoàn toàn với hộp sạc', 69.99, 150, GETDATE()),
(2, N'Tai nghe có dây 3.5mm', N'Tai nghe có dây chất lượng âm thanh cao', 24.99, 200, GETDATE()),
(3, N'Ốp lưng iPhone 14', N'Ốp lưng silicon bảo vệ chống sốc cho iPhone 14', 15.99, 300, GETDATE()),
(3, N'Ốp lưng Samsung Galaxy S23', N'Ốp lưng trong suốt chống trầy xước', 12.99, 280, GETDATE()),
(4, N'Bàn phím cơ RGB', N'Bàn phím cơ với đèn RGB tùy chỉnh', 89.99, 90, GETDATE()),
(4, N'Bàn phím không dây mini', N'Bàn phím không dây siêu mỏng cho văn phòng', 49.99, 110, GETDATE()),
(5, N'Chuột gaming không dây', N'Chuột gaming với cảm biến độ chính xác cao', 59.99, 130, GETDATE()),
(5, N'Chuột văn phòng có dây', N'Chuột quang cơ bản cho công việc văn phòng', 9.99, 400, GETDATE());