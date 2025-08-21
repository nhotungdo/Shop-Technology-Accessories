using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using ShopTechnology.DTOs;
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;

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
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);
            parameters.Add("@Password", password);

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_ValidateUser",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null)
            {
                return new LoginResult
                {
                    IsValid = false,
                    ErrorMessage = "User not found"
                };
            }

            var isValid = result.IsValid;
            var userId = result.UserId;
            var fullName = result.FullName;
            var roleId = result.RoleId;
            var roleName = result.RoleName;

            if (!isValid && !string.IsNullOrEmpty(password))
            {
                var user = await _context.Users
                    .Where(u => u.Email == email)
                    .Select(u => u.PasswordHash)
                    .FirstOrDefaultAsync();

                if (user != null && user.StartsWith("$2a$"))
                {
                    isValid = BCrypt.Net.BCrypt.Verify(password, user);
                }
            }

            return new LoginResult
            {
                IsValid = isValid,
                UserId = userId,
                FullName = fullName,
                RoleId = roleId,
                RoleName = roleName,
                ErrorMessage = isValid ? null : "Invalid password"
            };
        }
        catch (Exception)
        {
            return new LoginResult
            {
                IsValid = false,
                ErrorMessage = "Database error occurred"
            };
        }
    }

    public async Task<UserDTO?> GetUserByEmailAsync(string email)
    {
        try
        {
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_GetUserByEmail",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return null;

            return new UserDTO
            {
                UserId = result.UserId,
                FullName = result.FullName,
                Email = result.Email,
                PhoneNumber = result.PhoneNumber,
                RoleId = result.RoleId,
                RoleName = result.RoleName ?? string.Empty,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt
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
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@UserId", userId);

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_GetUserWithExternalLogins",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return null;

            return new UserDTO
            {
                UserId = result.UserId,
                FullName = result.FullName,
                Email = result.Email,
                PhoneNumber = result.PhoneNumber,
                RoleId = result.RoleId,
                RoleName = result.RoleName ?? string.Empty,
                CreatedAt = result.CreatedAt,
                UpdatedAt = result.UpdatedAt
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
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var sql = @"
                INSERT INTO LoginAttempts (Email, Success, IPAddress, UserAgent, AttemptTime)
                VALUES (@Email, @Success, @IPAddress, @UserAgent, @AttemptTime)";

            var parameters = new DynamicParameters();
            parameters.Add("@Email", email);
            parameters.Add("@Success", success);
            parameters.Add("@IPAddress", ipAddress);
            parameters.Add("@UserAgent", userAgent);
            parameters.Add("@AttemptTime", DateTime.UtcNow);

            await connection.ExecuteAsync(sql, parameters);
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
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var result = await connection.QueryFirstOrDefaultAsync<dynamic>(
                "sp_GetUserStatistics",
                commandType: CommandType.StoredProcedure
            );

            if (result == null) return new UserStatistics();

            return new UserStatistics
            {
                TotalUsers = result.TotalUsers,
                AdminUsers = result.AdminUsers,
                RegularUsers = result.RegularUsers,
                BCryptPasswords = result.BCryptPasswords,
                PlainTextPasswords = result.PlainTextPasswords,
                NullPasswords = result.NullPasswords
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
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();

            var parameters = new DynamicParameters();
            parameters.Add("@DaysToKeep", daysToKeep);

            await connection.ExecuteAsync(
                "sp_CleanupOldLoginAttempts",
                parameters,
                commandType: CommandType.StoredProcedure
            );

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }
}
