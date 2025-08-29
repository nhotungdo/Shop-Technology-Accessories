using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface IBannerService
    {
        Task<IEnumerable<Banner>> GetActiveBannersAsync();
        Task<Banner?> GetBannerByIdAsync(int id);
        Task<bool> CreateBannerAsync(Banner banner);
        Task<bool> UpdateBannerAsync(Banner banner);
        Task<bool> DeleteBannerAsync(int id);
        Task<bool> ToggleBannerStatusAsync(int id);
        Task<IEnumerable<Banner>> GetBannersByPositionAsync(string position);
    }
}
