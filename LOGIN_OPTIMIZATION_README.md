# Login System Optimization - Hướng dẫn sử dụng

## Tổng quan
Hệ thống login và register đã được tối ưu hóa hoàn toàn để phù hợp với database schema trong `Demo1_ShopTechnologyAccessories.sql`.

## Các cải tiến chính

### 🔧 **Database Optimization**
- **Stored Procedures**: Tối ưu hóa queries với stored procedures
- **Indexes**: Tạo indexes cho performance
- **Views**: Tạo views để thống kê
- **Functions**: Tạo functions để tính toán

### 🚀 **Performance Improvements**
- **Dapper Integration**: Sử dụng Dapper cho queries nhanh
- **Connection Pooling**: Tối ưu hóa connection management
- **Async Operations**: Tất cả operations đều async
- **Caching**: Response caching và compression

### 🔒 **Security Enhancements**
- **BCrypt Hashing**: Mã hóa mật khẩu an toàn
- **Input Validation**: Validation ở cả client và server
- **Session Management**: Quản lý session an toàn
- **Login Attempt Logging**: Ghi log các lần đăng nhập

## Database Schema

### Các bảng chính:
1. **Users** - Thông tin người dùng
2. **Roles** - Vai trò (Admin, User)
3. **ExternalLogins** - Đăng nhập OAuth
4. **PasswordResets** - Đặt lại mật khẩu
5. **LoginAttempts** - Log đăng nhập
6. **Promotions** - Mã giảm giá
7. **Reviews** - Đánh giá sản phẩm

### Stored Procedures:
- `sp_ValidateUser` - Validate user credentials
- `sp_GetUserByEmail` - Get user by email
- `sp_GetUserStatistics` - Get user statistics
- `sp_CleanupOldLoginAttempts` - Cleanup old logs
- `sp_ValidateExternalLogin` - Validate OAuth login
- `sp_GetUserWithExternalLogins` - Get user with OAuth info

### Views:
- `vw_UserSummary` - User summary view
- `ProductReviewSummary` - Product review statistics

## Cách sử dụng

### 1. **Chạy SQL Script**
```sql
-- Chạy file database
USE ShopTechnologyAccessories;
GO

-- Chạy optimization script
-- File: db/OptimizeLoginQueries.sql
```

### 2. **Chạy ứng dụng**
```bash
cd Shop-Technology-Accessories/ShopTechnology
dotnet run
```

### 3. **Truy cập**
- **Login**: `https://localhost:7062/Account/Login`
- **Register**: `https://localhost:7062/Account/Register`
- **Profile**: `https://localhost:7062/Account/Profile`

## Features

### 🔐 **Authentication**
- **Email/Password Login**: Đăng nhập thông thường
- **OAuth Login**: Google, Facebook
- **Remember Me**: Ghi nhớ đăng nhập
- **Password Reset**: Đặt lại mật khẩu qua email

### 👤 **User Management**
- **Registration**: Đăng ký tài khoản mới
- **Profile Management**: Quản lý thông tin cá nhân
- **Password Change**: Đổi mật khẩu
- **Account Deletion**: Xóa tài khoản

### 🛡️ **Security Features**
- **BCrypt Hashing**: Mã hóa mật khẩu
- **Input Validation**: Validation dữ liệu
- **Session Management**: Quản lý phiên đăng nhập
- **Login Attempt Logging**: Ghi log đăng nhập

### 🎨 **UI Features**
- **Modern Design**: Giao diện hiện đại
- **Responsive**: Tương thích mobile
- **Animations**: Hiệu ứng đẹp mắt
- **Loading States**: Trạng thái loading

### 🛠️ **Developer Tools**
- **Debug Tools**: Công cụ debug
- **User Testing**: Test user info
- **Statistics**: Thống kê người dùng
- **Admin Tools**: Công cụ admin

## API Endpoints

### Authentication
```
POST /Account/Login
POST /Account/Register
POST /Account/Logout
POST /Account/ForgotPassword
POST /Account/ResetPassword
```

### User Management
```
GET /Account/Profile
POST /Account/UpdateProfile
GET /Account/TestUserInfo
GET /Account/TestAllUsers
```

### Admin Tools
```
POST /Account/CreateRoles
POST /Account/CreateAdminUser
POST /Account/FixPasswordHashes
```

## Database Queries

### Login Validation
```sql
EXEC sp_ValidateUser @Email = 'user@example.com', @Password = 'password'
```

### Get User Info
```sql
EXEC sp_GetUserByEmail @Email = 'user@example.com'
```

### User Statistics
```sql
EXEC sp_GetUserStatistics
```

### Cleanup Old Logs
```sql
EXEC sp_CleanupOldLoginAttempts @DaysToKeep = 30
```

## Configuration

### Connection String
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=NHOTUNG\\SQLEXPRESS;Database=ShopTechnologyAccessories;User Id=sa;Password=123;TrustServerCertificate=true;Trusted_Connection=SSPI;Encrypt=false;"
  }
}
```

### Session Configuration
```csharp
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

## Testing

### Manual Testing
1. **Login Test**:
   - Email: `donhotung2004@gmail.com`
   - Password: `123456`

2. **Register Test**:
   - Tạo tài khoản mới
   - Verify email validation

3. **Password Reset Test**:
   - Request password reset
   - Check email token

### Debug Tools
1. **Test User Info**:
   - Click "Test Admin" button
   - Check user details

2. **Test All Users**:
   - Click "Test All Users" button
   - View user statistics

3. **Create Admin**:
   - Click "Create Admin" button
   - Create admin user

## Troubleshooting

### Common Issues

1. **Login Failed**:
   - Check database connection
   - Verify user exists
   - Check password hash

2. **Database Error**:
   - Run SQL scripts
   - Check connection string
   - Verify database exists

3. **Session Issues**:
   - Check session configuration
   - Clear browser cookies
   - Restart application

### Debug Steps

1. **Check Logs**:
   - Application logs
   - Database logs
   - Browser console

2. **Test Database**:
   - Run test queries
   - Check stored procedures
   - Verify indexes

3. **Test API**:
   - Use Postman
   - Check response codes
   - Verify data format

## Performance Tips

### Database
- Use stored procedures
- Create proper indexes
- Optimize queries
- Use connection pooling

### Application
- Enable caching
- Use async operations
- Optimize images
- Minify CSS/JS

### Security
- Use HTTPS
- Validate inputs
- Hash passwords
- Log security events

## Deployment

### Requirements
- .NET 8.0
- SQL Server
- IIS/Apache/Nginx
- SSL Certificate

### Steps
1. Build application
2. Deploy to server
3. Configure database
4. Set up SSL
5. Test functionality

## Support

### Documentation
- API Documentation
- Database Schema
- User Guide
- Developer Guide

### Contact
- Email: support@shoptech.com
- Phone: +84 931 982 568
- Website: https://shoptech.com

## Changelog

### Version 2.0.0 (Current)
- ✅ Complete login/register optimization
- ✅ Database schema alignment
- ✅ Performance improvements
- ✅ Security enhancements
- ✅ Modern UI/UX

### Version 1.0.0 (Previous)
- Basic login functionality
- Simple user management
- Basic security features

---

**Hệ thống login và register đã được tối ưu hóa hoàn toàn và sẵn sàng cho production!** 🚀
