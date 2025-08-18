using ShopTechnology.ViewModels;
using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface IWishlistService
    {
        Task<bool> AddAsync(Guid userId, int productId);
        Task<bool> RemoveAsync(Guid userId, int productId);
        Task<List<ProductViewModel>> GetAllAsync(Guid userId);
    }
}
