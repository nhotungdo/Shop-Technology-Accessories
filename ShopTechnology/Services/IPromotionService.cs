using ShopTechnology.DTOs;

namespace ShopTechnology.Services;

public interface IPromotionService
{
    Task<List<PromotionDTO>> GetAllPromotionsAsync();
    Task<PromotionDTO?> GetPromotionByIdAsync(int id);
    Task<PromotionDTO?> GetPromotionByCodeAsync(string code);
    Task<PromotionDTO> CreatePromotionAsync(CreatePromotionDTO createPromotionDto);
    Task<PromotionDTO> UpdatePromotionAsync(int id, UpdatePromotionDTO updatePromotionDto);
    Task<bool> DeletePromotionAsync(int id);
    Task<bool> ActivatePromotionAsync(int id);
    Task<bool> DeactivatePromotionAsync(int id);
    Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount);
    Task<bool> ValidatePromotionAsync(string code, decimal orderAmount);
    Task<bool> UsePromotionAsync(string code);
    Task<List<PromotionDTO>> GetActivePromotionsAsync();
    Task<int> GetTotalPromotionsCountAsync();
}
