-- ===============================
-- THÊM CÁC BẢNG CHO OAuth VÀ PASSWORD RESET
-- ===============================

USE ShopTechnologyAccessories;
GO

-- ===============================
-- 1. Bảng ExternalLogins (OAuth)
-- ===============================
CREATE TABLE ExternalLogins (
    ExternalLoginId INT PRIMARY KEY IDENTITY(1,1),
    UserId UNIQUEIDENTIFIER NOT NULL,
    Provider NVARCHAR(50) NOT NULL, -- Google, Facebook
    ProviderKey NVARCHAR(255) NOT NULL, -- ID từ provider
    Email NVARCHAR(255) NOT NULL,
    Name NVARCHAR(100) NOT NULL,
    PictureUrl NVARCHAR(500) NULL,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    LastLoginAt DATETIME NULL,
    
    CONSTRAINT FK_ExternalLogins_Users FOREIGN KEY (UserId) REFERENCES Users(UserId),
    CONSTRAINT UQ_ExternalLogins_Provider_Key UNIQUE (Provider, ProviderKey)
);
GO

-- ===============================
-- 2. Bảng PasswordResets
-- ===============================
CREATE TABLE PasswordResets (
    PasswordResetId INT PRIMARY KEY IDENTITY(1,1),
    Email NVARCHAR(255) NOT NULL,
    Token NVARCHAR(255) NOT NULL,
    ExpiresAt DATETIME NOT NULL,
    IsUsed BIT NOT NULL DEFAULT 0,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    UsedAt DATETIME NULL
);
GO

-- ===============================
-- 3. Tạo Index để tối ưu hiệu suất
-- ===============================
CREATE INDEX IX_ExternalLogins_UserId ON ExternalLogins(UserId);
CREATE INDEX IX_ExternalLogins_Provider ON ExternalLogins(Provider);
CREATE INDEX IX_ExternalLogins_Email ON ExternalLogins(Email);

CREATE INDEX IX_PasswordResets_Email ON PasswordResets(Email);
CREATE INDEX IX_PasswordResets_Token ON PasswordResets(Token);
CREATE INDEX IX_PasswordResets_Email_Token ON PasswordResets(Email, Token);
CREATE INDEX IX_PasswordResets_ExpiresAt ON PasswordResets(ExpiresAt);
GO

-- ===============================
-- 4. Thêm dữ liệu mẫu cho ExternalLogins (nếu cần)
-- ===============================
-- INSERT INTO ExternalLogins (UserId, Provider, ProviderKey, Email, Name, CreatedAt, LastLoginAt) VALUES
-- ((SELECT TOP 1 UserId FROM Users WHERE Email = 'user@shoptech.com'), 'Google', 'google_123456', 'user@shoptech.com', 'Test User', GETDATE(), GETDATE());
GO

-- ===============================
-- 5. Tạo Stored Procedure để cleanup expired tokens
-- ===============================
CREATE PROCEDURE CleanupExpiredPasswordResets
AS
BEGIN
    SET NOCOUNT ON;
    
    DELETE FROM PasswordResets 
    WHERE ExpiresAt < GETDATE() OR IsUsed = 1;
    
    PRINT 'Đã xóa các token hết hạn và đã sử dụng';
END
GO

-- ===============================
-- 6. Tạo Job để tự động cleanup (tùy chọn)
-- ===============================
-- EXEC sp_add_job
--     @job_name = N'CleanupExpiredPasswordResets',
--     @enabled = 1,
--     @description = N'Xóa các password reset token hết hạn';

-- EXEC sp_add_jobstep
--     @job_name = N'CleanupExpiredPasswordResets',
--     @step_name = N'Cleanup',
--     @subsystem = N'TSQL',
--     @command = N'EXEC CleanupExpiredPasswordResets';

-- EXEC sp_add_schedule
--     @schedule_name = N'DailyCleanup',
--     @freq_type = 4, -- Daily
--     @freq_interval = 1,
--     @active_start_time = 020000; -- 2:00 AM

-- EXEC sp_attach_schedule
--     @job_name = N'CleanupExpiredPasswordResets',
--     @schedule_name = N'DailyCleanup';
GO

PRINT 'Đã tạo thành công các bảng cho OAuth và Password Reset!';
PRINT 'Bao gồm:';
PRINT '- Bảng ExternalLogins (OAuth)';
PRINT '- Bảng PasswordResets';
PRINT '- Index tối ưu';
PRINT '- Stored Procedure cleanup';
PRINT '';
PRINT 'Lưu ý:';
PRINT '1. Cần cấu hình OAuth providers trong appsettings.json';
PRINT '2. Cần setup email service để gửi password reset';
PRINT '3. Có thể tạo SQL Agent Job để tự động cleanup tokens';
