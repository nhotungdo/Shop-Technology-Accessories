using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.DTOs;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class UserService : IUserService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IMapper _mapper;

    public UserService(ShopTechnologyAccessoriesContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<UserDTO?> GetUserByIdAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return _mapper.Map<UserDTO>(user);
    }

    public async Task<UserDTO?> GetUserByEmailAsync(string email)
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
                RoleName = u.Role != null ? u.Role.RoleName : string.Empty
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

    public async Task<List<UserDTO>> GetAllUsersAsync()
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return _mapper.Map<List<UserDTO>>(users);
    }

    public async Task<UserDTO> CreateUserAsync(CreateUserDTO createUserDto)
    {
        if (await IsEmailExistsAsync(createUserDto.Email))
        {
            throw new InvalidOperationException("Email already exists");
        }

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);

        var user = _mapper.Map<User>(createUserDto);
        user.PasswordHash = passwordHash;
        user.CreatedAt = DateTime.UtcNow;

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(user.UserId) ?? throw new InvalidOperationException("Failed to create user");
    }

    public async Task<UserDTO> UpdateUserAsync(Guid userId, UpdateUserDTO updateUserDto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        _mapper.Map(updateUserDto, user);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(userId) ?? throw new InvalidOperationException("Failed to update user");
    }

    public async Task<bool> DeleteUserAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid userId, ChangePasswordDTO changePasswordDto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        if (!BCrypt.Net.BCrypt.Verify(changePasswordDto.CurrentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(changePasswordDto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ValidateUserAsync(string email, string password)
    {
        var user = await _context.Users
            .Where(u => u.Email == email)
            .Select(u => new { u.PasswordHash, u.Email })
            .FirstOrDefaultAsync();

        if (user == null || string.IsNullOrEmpty(user.PasswordHash))
        {
            return false;
        }

        return !user.PasswordHash.StartsWith("$2a$")
            ? user.PasswordHash.Equals(password, StringComparison.OrdinalIgnoreCase)
            : BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
    }

    public async Task<bool> IsEmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }

    public async Task<bool> IsUserAdminAsync(Guid userId)
    {
        var user = await _context.Users
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.UserId == userId);

        return user?.Role?.RoleName == "Admin";
    }

    public async Task<List<UserDTO>> GetUsersByRoleAsync(string roleName)
    {
        var users = await _context.Users
            .Include(u => u.Role)
            .Where(u => u.Role.RoleName == roleName)
            .OrderBy(u => u.FullName)
            .ToListAsync();

        return _mapper.Map<List<UserDTO>>(users);
    }

    public async Task<int> GetTotalUsersCountAsync()
    {
        return await _context.Users.CountAsync();
    }

    public async Task<bool> FixPasswordHashesAsync()
    {
        var users = await _context.Users.ToListAsync();
        var hasChanges = false;

        foreach (var user in users)
        {
            if (string.IsNullOrEmpty(user.PasswordHash) || !user.PasswordHash.StartsWith("$2a$"))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.PasswordHash ?? "123456");
                user.UpdatedAt = DateTime.UtcNow;
                hasChanges = true;
            }
        }

        if (hasChanges)
        {
            await _context.SaveChangesAsync();
        }

        return hasChanges;
    }

    public async Task<bool> ForgotPasswordAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        var token = Guid.NewGuid().ToString("N");
        var expiresAt = DateTime.UtcNow.AddHours(24);

        var passwordReset = new PasswordReset
        {
            Email = email,
            Token = token,
            ExpiresAt = expiresAt,
            IsUsed = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.PasswordResets.Add(passwordReset);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
    {
        var resetRecord = await _context.PasswordResets
            .FirstOrDefaultAsync(pr => pr.Email == email && pr.Token == token && pr.IsUsed == false && pr.ExpiresAt > DateTime.UtcNow);

        if (resetRecord == null) return false;

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        resetRecord.IsUsed = true;
        resetRecord.UsedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ValidateResetTokenAsync(string email, string token)
    {
        var resetRecord = await _context.PasswordResets
            .FirstOrDefaultAsync(pr => pr.Email == email && pr.Token == token && pr.IsUsed == false && pr.ExpiresAt > DateTime.UtcNow);

        return resetRecord != null;
    }

    public async Task<UserDTO?> GetUserByExternalLoginAsync(string provider, string providerKey)
    {
        var externalLogin = await _context.ExternalLogins
            .Include(el => el.User)
            .ThenInclude(u => u.Role)
            .FirstOrDefaultAsync(el => el.Provider == provider && el.ProviderKey == providerKey);

        if (externalLogin == null) return null;

        externalLogin.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return _mapper.Map<UserDTO>(externalLogin.User);
    }

    public async Task<UserDTO> CreateUserFromExternalLoginAsync(string provider, string providerKey, string email, string name, string? pictureUrl)
    {
        var user = new User
        {
            Email = email,
            FullName = name,
            RoleId = 2,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var externalLogin = new ExternalLogin
        {
            UserId = user.UserId,
            Provider = provider,
            ProviderKey = providerKey,
            Email = email,
            Name = name,
            PictureUrl = pictureUrl,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        _context.ExternalLogins.Add(externalLogin);
        await _context.SaveChangesAsync();

        return await GetUserByIdAsync(user.UserId) ?? throw new InvalidOperationException("Failed to create user");
    }

    public async Task<bool> LinkExternalLoginAsync(Guid userId, string provider, string providerKey, string email, string name, string? pictureUrl)
    {
        var externalLogin = new ExternalLogin
        {
            UserId = userId,
            Provider = provider,
            ProviderKey = providerKey,
            Email = email,
            Name = name,
            PictureUrl = pictureUrl,
            CreatedAt = DateTime.UtcNow,
            LastLoginAt = DateTime.UtcNow
        };

        _context.ExternalLogins.Add(externalLogin);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CreateAdminUserAsync()
    {
        var existingAdmin = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == "donhotung2004@gmail.com");

        if (existingAdmin != null) return false;

        var adminUser = new User
        {
            UserId = Guid.NewGuid(),
            FullName = "Admin",
            Email = "donhotung2004@gmail.com",
            PasswordHash = "123456",
            PhoneNumber = "0931982568",
            RoleId = 1,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(adminUser);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> CreateRolesAsync()
    {
        var existingRoles = await _context.Roles.ToListAsync();
        if (existingRoles.Any()) return false;

        var adminRole = new Role { RoleId = 1, RoleName = "Admin" };
        var userRole = new Role { RoleId = 2, RoleName = "User" };

        _context.Roles.Add(adminRole);
        _context.Roles.Add(userRole);
        await _context.SaveChangesAsync();

        return true;
    }
}
