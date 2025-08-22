using ShopTechnology.Models;

namespace ShopTechnology.Services;

public interface IPromotionService
{
    Task<List<Promotion>> GetAllPromotionsAsync();
    Task<Promotion?> GetPromotionByIdAsync(int id);
    Task<Promotion?> GetPromotionByCodeAsync(string code);
    Task<Promotion> CreatePromotionAsync(Promotion promotion);
    Task<bool> UpdatePromotionAsync(Promotion promotion);
    Task<bool> DeletePromotionAsync(int id);
    Task<bool> ValidatePromotionAsync(string code, decimal orderAmount);
    Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount);
    Task<bool> UsePromotionAsync(string code);
    Task<List<Promotion>> GetActivePromotionsAsync();
    Task<List<Promotion>> GetExpiredPromotionsAsync();
    Task<List<Promotion>> GetUpcomingPromotionsAsync();
    Task<bool> TogglePromotionStatusAsync(int id);
    Task<int> GetTotalPromotionsCountAsync();
    Task<int> GetActivePromotionsCountAsync();
    Task<decimal> GetTotalDiscountUsedAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
}
