using Microsoft.EntityFrameworkCore;
using ShopTechnology.Data;
using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public class BannerService : IBannerService
    {
        private readonly ApplicationDbContext _context;

        public BannerService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Banner>> GetActiveBannersAsync()
        {
            return await _context.Banners
                .Where(b => b.IsActive && 
                           b.StartDate <= DateTime.UtcNow && 
                           (b.EndDate == null || b.EndDate >= DateTime.UtcNow))
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }

        public async Task<Banner?> GetBannerByIdAsync(int id)
        {
            return await _context.Banners.FindAsync(id);
        }

        public async Task<bool> CreateBannerAsync(Banner banner)
        {
            try
            {
                banner.CreatedAt = DateTime.UtcNow;
                _context.Banners.Add(banner);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> UpdateBannerAsync(Banner banner)
        {
            try
            {
                banner.UpdatedAt = DateTime.UtcNow;
                _context.Banners.Update(banner);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteBannerAsync(int id)
        {
            try
            {
                var banner = await _context.Banners.FindAsync(id);
                if (banner == null) return false;

                _context.Banners.Remove(banner);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleBannerStatusAsync(int id)
        {
            try
            {
                var banner = await _context.Banners.FindAsync(id);
                if (banner == null) return false;

                banner.IsActive = !banner.IsActive;
                banner.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<IEnumerable<Banner>> GetBannersByPositionAsync(string position)
        {
            return await _context.Banners
                .Where(b => b.IsActive && 
                           b.StartDate <= DateTime.UtcNow && 
                           (b.EndDate == null || b.EndDate >= DateTime.UtcNow))
                .OrderBy(b => b.DisplayOrder)
                .ToListAsync();
        }
    }
}
