-- =============================================
-- SQL Script để tối ưu hóa Login System
-- =============================================

USE ShopTechnologyAccessories;
GO

-- 1. Tạo indexes để tối ưu hóa query đăng nhập
-- Index cho email (unique constraint)
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_Email' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE UNIQUE INDEX IX_Users_Email ON Users(Email);
    PRINT 'Created unique index on Users.Email';
END

-- Index cho RoleId để join với Roles table
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_RoleId' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE INDEX IX_Users_RoleId ON Users(RoleId);
    PRINT 'Created index on Users.RoleId';
END

-- Index cho CreatedAt để sắp xếp
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_CreatedAt' AND object_id = OBJECT_ID('Users'))
BEGIN
    CREATE INDEX IX_Users_CreatedAt ON Users(CreatedAt);
    PRINT 'Created index on Users.CreatedAt';
END

-- 2. Tạo stored procedure cho login validation
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ValidateUser')
    DROP PROCEDURE sp_ValidateUser;
GO

CREATE PROCEDURE sp_ValidateUser
    @Email NVARCHAR(255),
    @Password NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    DECLARE @UserId UNIQUEIDENTIFIER;
    DECLARE @PasswordHash NVARCHAR(255);
    DECLARE @FullName NVARCHAR(100);
    DECLARE @RoleId INT;
    DECLARE @RoleName NVARCHAR(50);
    DECLARE @IsValid BIT = 0;
    
    -- Lấy thông tin user
    SELECT 
        @UserId = u.UserId,
        @PasswordHash = u.PasswordHash,
        @FullName = u.FullName,
        @RoleId = u.RoleId,
        @RoleName = r.RoleName
    FROM Users u
    LEFT JOIN Roles r ON u.RoleId = r.RoleId
    WHERE u.Email = @Email;
    
    -- Kiểm tra user tồn tại
    IF @UserId IS NOT NULL
    BEGIN
        -- Kiểm tra password
        IF @PasswordHash IS NOT NULL
        BEGIN
            -- Nếu password hash không phải BCrypt format, so sánh trực tiếp
            IF NOT @PasswordHash LIKE '$2a$%'
            BEGIN
                IF @PasswordHash = @Password
                    SET @IsValid = 1;
            END
            ELSE
            BEGIN
                -- Sử dụng BCrypt (cần implement trong C#)
                -- Ở đây chỉ trả về thông tin để C# xử lý
                SET @IsValid = 0; -- Sẽ được xử lý trong C#
            END
        END
    END
    
    -- Trả về kết quả
    SELECT 
        @IsValid AS IsValid,
        @UserId AS UserId,
        @FullName AS FullName,
        @RoleId AS RoleId,
        @RoleName AS RoleName;
END
GO

-- 3. Tạo stored procedure để lấy user info
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetUserByEmail')
    DROP PROCEDURE sp_GetUserByEmail;
GO

CREATE PROCEDURE sp_GetUserByEmail
    @Email NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.FullName,
        u.Email,
        u.PhoneNumber,
        u.RoleId,
        u.CreatedAt,
        u.UpdatedAt,
        r.RoleName
    FROM Users u
    LEFT JOIN Roles r ON u.RoleId = r.RoleId
    WHERE u.Email = @Email;
END
GO

-- 4. Tạo view để hiển thị user summary
IF EXISTS (SELECT * FROM sys.views WHERE name = 'vw_UserSummary')
    DROP VIEW vw_UserSummary;
GO

CREATE VIEW vw_UserSummary AS
SELECT 
    u.UserId,
    u.FullName,
    u.Email,
    u.PhoneNumber,
    u.RoleId,
    r.RoleName,
    CASE 
        WHEN u.PasswordHash LIKE '$2a$%' THEN 'BCrypt'
        WHEN u.PasswordHash IS NOT NULL THEN 'Plain Text'
        ELSE 'NULL'
    END AS PasswordType,
    CASE 
        WHEN u.PasswordHash IS NOT NULL THEN LEFT(u.PasswordHash, 10) + '...'
        ELSE 'NULL'
    END AS PasswordPreview,
    u.CreatedAt,
    u.UpdatedAt
FROM Users u
LEFT JOIN Roles r ON u.RoleId = r.RoleId;
GO

-- 5. Tạo function để đếm users theo role
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'FN' AND name = 'fn_GetUserCountByRole')
    DROP FUNCTION fn_GetUserCountByRole;
GO

CREATE FUNCTION fn_GetUserCountByRole(@RoleName NVARCHAR(50))
RETURNS INT
AS
BEGIN
    DECLARE @Count INT;
    
    SELECT @Count = COUNT(*)
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    WHERE r.RoleName = @RoleName;
    
    RETURN @Count;
END
GO

-- 6. Tạo table để log login attempts (nếu chưa có)
IF NOT EXISTS (SELECT * FROM sys.objects WHERE type = 'U' AND name = 'LoginAttempts')
BEGIN
    CREATE TABLE LoginAttempts (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Email NVARCHAR(255),
        AttemptTime DATETIME2 DEFAULT GETDATE(),
        Success BIT,
        IPAddress NVARCHAR(45),
        UserAgent NVARCHAR(500)
    );
    PRINT 'Created LoginAttempts table';
END

-- 7. Tạo index cho LoginAttempts table
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LoginAttempts_Email' AND object_id = OBJECT_ID('LoginAttempts'))
BEGIN
    CREATE INDEX IX_LoginAttempts_Email ON LoginAttempts(Email);
    PRINT 'Created index on LoginAttempts.Email';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_LoginAttempts_AttemptTime' AND object_id = OBJECT_ID('LoginAttempts'))
BEGIN
    CREATE INDEX IX_LoginAttempts_AttemptTime ON LoginAttempts(AttemptTime);
    PRINT 'Created index on LoginAttempts.AttemptTime';
END

-- 8. Tạo procedure để cleanup old login attempts
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CleanupOldLoginAttempts')
    DROP PROCEDURE sp_CleanupOldLoginAttempts;
GO

CREATE PROCEDURE sp_CleanupOldLoginAttempts
    @DaysToKeep INT = 30
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM LoginAttempts 
    WHERE AttemptTime < DATEADD(DAY, -@DaysToKeep, GETDATE());
    
    PRINT 'Cleaned up old login attempts';
END
GO

-- 9. Tạo procedure để get user statistics
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetUserStatistics')
    DROP PROCEDURE sp_GetUserStatistics;
GO

CREATE PROCEDURE sp_GetUserStatistics
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        COUNT(*) AS TotalUsers,
        SUM(CASE WHEN r.RoleName = 'Admin' THEN 1 ELSE 0 END) AS AdminUsers,
        SUM(CASE WHEN r.RoleName = 'User' THEN 1 ELSE 0 END) AS RegularUsers,
        SUM(CASE WHEN u.PasswordHash LIKE '$2a$%' THEN 1 ELSE 0 END) AS BCryptPasswords,
        SUM(CASE WHEN u.PasswordHash IS NOT NULL AND u.PasswordHash NOT LIKE '$2a$%' THEN 1 ELSE 0 END) AS PlainTextPasswords,
        SUM(CASE WHEN u.PasswordHash IS NULL THEN 1 ELSE 0 END) AS NullPasswords
    FROM Users u
    LEFT JOIN Roles r ON u.RoleId = r.RoleId;
END
GO

-- 10. Tạo procedure để cleanup expired password resets
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CleanupExpiredPasswordResets')
    DROP PROCEDURE sp_CleanupExpiredPasswordResets;
GO

CREATE PROCEDURE sp_CleanupExpiredPasswordResets
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM PasswordResets 
    WHERE ExpiresAt < GETDATE() OR IsUsed = 1;
    
    PRINT 'Cleaned up expired password resets';
END
GO

-- 11. Tạo procedure để validate external login
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_ValidateExternalLogin')
    DROP PROCEDURE sp_ValidateExternalLogin;
GO

CREATE PROCEDURE sp_ValidateExternalLogin
    @Provider NVARCHAR(50),
    @ProviderKey NVARCHAR(255)
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        el.ExternalLoginId,
        el.UserId,
        el.Email,
        el.Name,
        el.PictureUrl,
        u.FullName,
        u.RoleId,
        r.RoleName
    FROM ExternalLogins el
    INNER JOIN Users u ON el.UserId = u.UserId
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    WHERE el.Provider = @Provider AND el.ProviderKey = @ProviderKey;
END
GO

-- 12. Tạo procedure để get user with external logins
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_GetUserWithExternalLogins')
    DROP PROCEDURE sp_GetUserWithExternalLogins;
GO

CREATE PROCEDURE sp_GetUserWithExternalLogins
    @UserId UNIQUEIDENTIFIER
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        u.UserId,
        u.FullName,
        u.Email,
        u.PhoneNumber,
        u.RoleId,
        r.RoleName,
        u.CreatedAt,
        u.UpdatedAt,
        el.Provider,
        el.ProviderKey,
        el.PictureUrl
    FROM Users u
    INNER JOIN Roles r ON u.RoleId = r.RoleId
    LEFT JOIN ExternalLogins el ON u.UserId = el.UserId
    WHERE u.UserId = @UserId;
END
GO

-- 13. Tạo view để thống kê đánh giá sản phẩm
IF EXISTS (SELECT * FROM sys.views WHERE name = 'ProductReviewSummary')
    DROP VIEW ProductReviewSummary;
GO

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

-- 14. Tạo procedure để tính promotion discount
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'CalculatePromotionDiscount')
    DROP PROCEDURE CalculatePromotionDiscount;
GO

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

PRINT 'All optimization scripts completed successfully!';
GO
