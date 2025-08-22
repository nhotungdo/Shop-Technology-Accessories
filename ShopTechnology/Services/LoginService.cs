using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.DTOs;

namespace ShopTechnology.Services;

public interface ILoginService
{
    Task<LoginResult> ValidateUserAsync(string email, string password);
    Task<UserDTO?> GetUserByEmailAsync(string email);
    Task<UserDTO?> GetUserWithExternalLoginsAsync(Guid userId);
    Task<bool> LogLoginAttemptAsync(string email, bool success, string ipAddress, string userAgent);
    Task<UserStatistics> GetUserStatisticsAsync();
    Task<bool> CleanupOldLoginAttemptsAsync(int daysToKeep = 30);
}

public class LoginResult
{
    public bool IsValid { get; set; }
    public Guid? UserId { get; set; }
    public string? FullName { get; set; }
    public int? RoleId { get; set; }
    public string? RoleName { get; set; }
    public string? ErrorMessage { get; set; }
}

public class UserStatistics
{
    public int TotalUsers { get; set; }
    public int AdminUsers { get; set; }
    public int RegularUsers { get; set; }
    public int BCryptPasswords { get; set; }
    public int PlainTextPasswords { get; set; }
    public int NullPasswords { get; set; }
}

public class LoginService : ILoginService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly string _connectionString;

    public LoginService(ShopTechnologyAccessoriesContext context, IConfiguration configuration)
    {
        _context = context;
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task<LoginResult> ValidateUserAsync(string email, string password)
    {
        try
        {
            // Tìm user theo email
            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Email == email)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.PasswordHash,
                    u.RoleId,
                    RoleName = u.Role != null ? u.Role.RoleName : "User"
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return new LoginResult
                {
                    IsValid = false,
                    ErrorMessage = "Email không tồn tại trong hệ thống"
                };
            }

            bool isValid = false;

            // Kiểm tra password
            if (!string.IsNullOrEmpty(user.PasswordHash))
            {
                if (user.PasswordHash.StartsWith("$2a$"))
                {
                    // Password đã được hash bằng BCrypt
                    isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
                }
                else
                {
                    // Password plain text (cho backward compatibility)
                    isValid = user.PasswordHash.Equals(password, StringComparison.OrdinalIgnoreCase);
                }
            }

            return new LoginResult
            {
                IsValid = isValid,
                UserId = user.UserId,
                FullName = user.FullName,
                RoleId = user.RoleId,
                RoleName = user.RoleName,
                ErrorMessage = isValid ? null : "Mật khẩu không đúng"
            };
        }
        catch (Exception ex)
        {
            return new LoginResult
            {
                IsValid = false,
                ErrorMessage = $"Lỗi xác thực: {ex.Message}"
            };
        }
    }

    public async Task<UserDTO?> GetUserByEmailAsync(string email)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.Email == email)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    u.RoleId,
                    u.CreatedAt,
                    u.UpdatedAt,
                    RoleName = u.Role != null ? u.Role.RoleName : "User"
                })
                .FirstOrDefaultAsync();

            if (user == null) return null;

            return new UserDTO
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                RoleName = user.RoleName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<UserDTO?> GetUserWithExternalLoginsAsync(Guid userId)
    {
        try
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .Where(u => u.UserId == userId)
                .Select(u => new
                {
                    u.UserId,
                    u.FullName,
                    u.Email,
                    u.PhoneNumber,
                    u.RoleId,
                    u.CreatedAt,
                    u.UpdatedAt,
                    RoleName = u.Role != null ? u.Role.RoleName : "User"
                })
                .FirstOrDefaultAsync();

            if (user == null) return null;

            return new UserDTO
            {
                UserId = user.UserId,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                RoleId = user.RoleId,
                RoleName = user.RoleName,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<bool> LogLoginAttemptAsync(string email, bool success, string ipAddress, string userAgent)
    {
        try
        {
            // Tạo log đơn giản bằng console hoặc có thể tạo bảng LoginAttempts sau
            Console.WriteLine($"Login attempt - Email: {email}, Success: {success}, IP: {ipAddress}, Time: {DateTime.Now}");
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public async Task<UserStatistics> GetUserStatisticsAsync()
    {
        try
        {
            var users = await _context.Users.ToListAsync();

            return new UserStatistics
            {
                TotalUsers = users.Count,
                AdminUsers = users.Count(u => u.RoleId == 1),
                RegularUsers = users.Count(u => u.RoleId == 2),
                BCryptPasswords = users.Count(u => u.PasswordHash != null && u.PasswordHash.StartsWith("$2a$")),
                PlainTextPasswords = users.Count(u => u.PasswordHash != null && !u.PasswordHash.StartsWith("$2a$")),
                NullPasswords = users.Count(u => string.IsNullOrEmpty(u.PasswordHash))
            };
        }
        catch (Exception)
        {
            return new UserStatistics();
        }
    }

    public async Task<bool> CleanupOldLoginAttemptsAsync(int daysToKeep = 30)
    {
        try
        {
            // Tạm thời return true vì chưa có bảng LoginAttempts
            // Có thể implement sau khi tạo bảng LoginAttempts
            Console.WriteLine($"Cleanup old login attempts - keeping {daysToKeep} days");
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
