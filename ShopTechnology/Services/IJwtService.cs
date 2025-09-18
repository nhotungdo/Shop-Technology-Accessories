using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface IJwtService
    {
        string GenerateToken(User user);
        bool ValidateToken(string token);
        string? GetUserIdFromToken(string token);
        string? GetUserRoleFromToken(string token);
    }
}
