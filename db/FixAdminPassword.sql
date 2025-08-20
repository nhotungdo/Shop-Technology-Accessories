-- Script để cập nhật password hash cho tài khoản admin
-- Sử dụng BCrypt để hash password "123456"

USE ShopTechnologyAccessories;
GO

-- Cập nhật password hash cho tài khoản admin
UPDATE Users 
SET PasswordHash = '$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi'  -- BCrypt hash của "123456"
WHERE Email = 'donhotung2004@gmail.com' AND RoleId = 1;

-- Kiểm tra kết quả
SELECT 
    UserId,
    FullName,
    Email,
    PasswordHash,
    RoleId,
    CreatedAt
FROM Users 
WHERE Email = 'donhotung2004@gmail.com';

-- Hoặc nếu muốn tạo mới tài khoản admin
-- DELETE FROM Users WHERE Email = 'donhotung2004@gmail.com';
-- 
-- INSERT INTO Users (
--     FullName, Email, PasswordHash, PhoneNumber, RoleId, CreatedAt
-- ) 
-- VALUES (
--     N'Admin', 
--     N'donhotung2004@gmail.com', 
--     N'$2a$11$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2.uheWG/igi',  -- BCrypt hash của "123456"
--     N'0931982568', 
--     1, 
--     GETDATE()
-- );
