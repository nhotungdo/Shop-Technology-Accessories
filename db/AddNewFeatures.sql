-- ===============================
-- THÊM CÁC BẢNG MỚI CHO CHỨC NĂNG NÂNG CAO
-- ===============================

USE ShopTechnologyAccessories;
GO

-- ===============================
-- 1. Bảng Promotions (Mã giảm giá)
-- ===============================
CREATE TABLE Promotions (
    PromotionId INT PRIMARY KEY IDENTITY(1,1),
    Code NVARCHAR(20) NOT NULL UNIQUE,
    Name NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500) NULL,
    DiscountAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    DiscountPercentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    MinimumOrderAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    MaxUsageCount INT NOT NULL DEFAULT 1,
    UsedCount INT NOT NULL DEFAULT 0,
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UpdatedAt DATETIME NULL
);
GO

-- ===============================
-- 2. Bảng Reviews (Đánh giá sản phẩm)
-- ===============================
CREATE TABLE Reviews (
    ReviewId INT PRIMARY KEY IDENTITY(1,1),
    UserId UNIQUEIDENTIFIER NOT NULL,
    ProductId INT NOT NULL,
    Rating INT NOT NULL CHECK (Rating >= 1 AND Rating <= 5),
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
-- 3. Thêm dữ liệu mẫu cho Promotions
-- ===============================
INSERT INTO Promotions (Code, Name, Description, DiscountAmount, DiscountPercentage, MinimumOrderAmount, MaxUsageCount, StartDate, EndDate, IsActive) VALUES
('WELCOME10', N'Chào mừng khách hàng mới', N'Giảm 10% cho đơn hàng đầu tiên', 0, 10.00, 100000, 1, GETDATE(), DATEADD(MONTH, 6, GETDATE()), 1),
('SAVE20', N'Tiết kiệm 20%', N'Giảm 20% cho đơn hàng từ 500k', 0, 20.00, 500000, 100, GETDATE(), DATEADD(MONTH, 3, GETDATE()), 1),
('FREESHIP', N'Miễn phí vận chuyển', N'Miễn phí vận chuyển cho đơn hàng từ 300k', 50000, 0, 300000, 50, GETDATE(), DATEADD(MONTH, 2, GETDATE()), 1),
('FLASH50', N'Flash Sale 50%', N'Giảm 50% cho các sản phẩm được chọn', 0, 50.00, 200000, 20, GETDATE(), DATEADD(DAY, 7, GETDATE()), 1);
GO

-- ===============================
-- 4. Thêm dữ liệu mẫu cho Reviews
-- ===============================
INSERT INTO Reviews (UserId, ProductId, Rating, Comment, IsVerified, CreatedAt) VALUES
-- Lấy UserId từ bảng Users (thay thế bằng UserId thực tế)
((SELECT TOP 1 UserId FROM Users WHERE Email = 'user@shoptech.com'), 1, 5, N'Sản phẩm chất lượng tốt, giao hàng nhanh!', 1, GETDATE()),
((SELECT TOP 1 UserId FROM Users WHERE Email = 'user@shoptech.com'), 2, 4, N'Sản phẩm đẹp, giá cả hợp lý', 1, GETDATE()),
((SELECT TOP 1 UserId FROM Users WHERE Email = 'user@shoptech.com'), 3, 5, N'Rất hài lòng với sản phẩm này', 1, GETDATE()),
((SELECT TOP 1 UserId FROM Users WHERE Email = 'admin@shoptech.com'), 1, 4, N'Chất lượng tốt, đóng gói cẩn thận', 1, GETDATE()),
((SELECT TOP 1 UserId FROM Users WHERE Email = 'admin@shoptech.com'), 4, 5, N'Sản phẩm vượt mong đợi!', 1, GETDATE());
GO

-- ===============================
-- 5. Tạo Index để tối ưu hiệu suất
-- ===============================
CREATE INDEX IX_Promotions_Code ON Promotions(Code);
CREATE INDEX IX_Promotions_IsActive ON Promotions(IsActive);
CREATE INDEX IX_Promotions_StartDate ON Promotions(StartDate);
CREATE INDEX IX_Promotions_EndDate ON Promotions(EndDate);

CREATE INDEX IX_Reviews_ProductId ON Reviews(ProductId);
CREATE INDEX IX_Reviews_UserId ON Reviews(UserId);
CREATE INDEX IX_Reviews_Rating ON Reviews(Rating);
CREATE INDEX IX_Reviews_CreatedAt ON Reviews(CreatedAt);
GO

-- ===============================
-- 6. Tạo View để thống kê đánh giá
-- ===============================
CREATE VIEW ProductReviewSummary AS
SELECT 
    p.ProductId,
    p.ProductName,
    AVG(CAST(r.Rating AS FLOAT)) AS AverageRating,
    COUNT(r.ReviewId) AS TotalReviews,
    SUM(CASE WHEN r.Rating = 5 THEN 1 ELSE 0 END) AS FiveStarCount,
    SUM(CASE WHEN r.Rating = 4 THEN 1 ELSE 0 END) AS FourStarCount,
    SUM(CASE WHEN r.Rating = 3 THEN 1 ELSE 0 END) AS ThreeStarCount,
    SUM(CASE WHEN r.Rating = 2 THEN 1 ELSE 0 END) AS TwoStarCount,
    SUM(CASE WHEN r.Rating = 1 THEN 1 ELSE 0 END) AS OneStarCount
FROM Products p
LEFT JOIN Reviews r ON p.ProductId = r.ProductId
GROUP BY p.ProductId, p.ProductName;
GO

-- ===============================
-- 7. Tạo Stored Procedure để tính discount
-- ===============================
CREATE PROCEDURE CalculatePromotionDiscount
    @PromotionCode NVARCHAR(20),
    @OrderAmount DECIMAL(18,2),
    @DiscountAmount DECIMAL(18,2) OUTPUT
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @PromotionId INT, @DiscountPercent DECIMAL(5,2), @MinAmount DECIMAL(18,2);
    DECLARE @IsActive BIT, @UsedCount INT, @MaxUsage INT;
    DECLARE @StartDate DATETIME, @EndDate DATETIME;
    
    -- Lấy thông tin promotion
    SELECT 
        @PromotionId = PromotionId,
        @DiscountPercent = DiscountPercentage,
        @MinAmount = MinimumOrderAmount,
        @IsActive = IsActive,
        @UsedCount = UsedCount,
        @MaxUsage = MaxUsageCount,
        @StartDate = StartDate,
        @EndDate = EndDate
    FROM Promotions 
    WHERE Code = @PromotionCode;
    
    -- Kiểm tra promotion có hợp lệ không
    IF @PromotionId IS NULL OR 
       @IsActive = 0 OR 
       @UsedCount >= @MaxUsage OR
       GETDATE() < @StartDate OR 
       GETDATE() > @EndDate OR
       @OrderAmount < @MinAmount
    BEGIN
        SET @DiscountAmount = 0;
        RETURN;
    END
    
    -- Tính discount
    SET @DiscountAmount = @OrderAmount * (@DiscountPercent / 100);
    
    -- Đảm bảo discount không vượt quá order amount
    IF @DiscountAmount > @OrderAmount
        SET @DiscountAmount = @OrderAmount;
END
GO

-- ===============================
-- 8. Tạo Trigger để cập nhật UsedCount
-- ===============================
-- CREATE TRIGGER TR_Promotions_UpdateUsedCount
-- ON Orders
-- AFTER INSERT
-- AS
-- BEGIN
--     -- Logic để cập nhật UsedCount khi order được tạo
--     -- (Cần thêm PromotionId vào bảng Orders nếu muốn track)
-- END
GO

PRINT 'Đã tạo thành công các bảng và dữ liệu mẫu cho chức năng nâng cao!';
PRINT 'Bao gồm:';
PRINT '- Bảng Promotions (Mã giảm giá)';
PRINT '- Bảng Reviews (Đánh giá sản phẩm)';
PRINT '- Dữ liệu mẫu';
PRINT '- Index và View tối ưu';
PRINT '- Stored Procedure tính discount';
