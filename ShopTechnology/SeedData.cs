using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;
using System.Security.Cryptography;
using System.Text;

namespace ShopTechnology
{
    public static class SeedData
    {
        public static async Task Initialize(ShopTechnologyAccessoriesContext context)
        {
            // Tạo role Admin nếu chưa có
            if (!await context.Roles.AnyAsync(r => r.Name == "Admin"))
            {
                await context.Roles.AddAsync(new Role { Name = "Admin", CreatedAt = DateTime.Now });
                await context.SaveChangesAsync();
            }

            // Tạo role User nếu chưa có
            if (!await context.Roles.AnyAsync(r => r.Name == "User"))
            {
                await context.Roles.AddAsync(new Role { Name = "User", CreatedAt = DateTime.Now });
                await context.SaveChangesAsync();
            }

            // Tạo user admin nếu chưa tồn tại
            var adminEmail = "donhotung2004@gmail.com";
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.Email == adminEmail);
            if (adminUser == null)
            {
                var adminRole = await context.Roles.FirstAsync(r => r.Name == "Admin");
                adminUser = new User
                {
                    RoleId = adminRole.RoleId,
                    FullName = "Admin",
                    Email = adminEmail,
                    PhoneNumber = "0931982568",
                    Password = HashPassword("Donhotung2004"),
                    DateOfBirth = new DateTime(1990, 1, 1),
                    IsEmailVerified = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();
            }
        }

        private static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }
}
