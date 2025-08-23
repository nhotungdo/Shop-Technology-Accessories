using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface IBannerService
    {
        Task<List<Banner>> GetActiveBannersAsync(string position);
        Task<List<Banner>> GetAllBannersAsync();
        Task<Banner?> GetBannerByIdAsync(int bannerId);
    }
}
