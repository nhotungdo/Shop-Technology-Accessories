using ShopTechnology.Models;

namespace ShopTechnology.Services
{
    public interface IPromotionService
    {
        Task<IEnumerable<Promotion>> GetActivePromotionsAsync();
        Task<Promotion?> GetPromotionByCodeAsync(string code);
        Task<Promotion?> GetPromotionByIdAsync(int id);
        Task<bool> CreatePromotionAsync(Promotion promotion);
        Task<bool> UpdatePromotionAsync(Promotion promotion);
        Task<bool> DeletePromotionAsync(int id);
        Task<bool> ValidatePromotionAsync(string code, string userId, decimal orderAmount);
        Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount);
        Task<bool> ApplyPromotionToOrderAsync(int orderId, string code);
        Task<bool> IncrementUsageCountAsync(string code);
        Task<IEnumerable<Promotion>> GetPromotionsByProductAsync(int productId);
        Task<bool> AddProductToPromotionAsync(int promotionId, int productId);
        Task<bool> RemoveProductFromPromotionAsync(int promotionId, int productId);
    }
}
