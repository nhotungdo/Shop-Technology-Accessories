using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public class BannerService : IBannerService
    {
        private readonly ShopTechnologyAccessoriesContext _context;

        public BannerService(ShopTechnologyAccessoriesContext context)
        {
            _context = context;
        }

        public async Task<List<Banner>> GetActiveBannersAsync(string position)
        {
            return await _context.Banners
                .Where(b => true /* b.IsActive - removed because column doesn't exist */ && 
                           b.Position == position &&
                           b.StartDate <= DateTime.Now && 
                           b.EndDate >= DateTime.Now)
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }

        public async Task<List<Banner>> GetAllBannersAsync()
        {
            return await _context.Banners
                .OrderBy(b => b.DisplayOrder)
                .ThenBy(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Banner?> GetBannerByIdAsync(int bannerId)
        {
            return await _context.Banners.FindAsync(bannerId);
        }
    }
}
