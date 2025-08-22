using Microsoft.EntityFrameworkCore;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class PromotionService : IPromotionService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly ILogger<PromotionService> _logger;

    public PromotionService(ShopTechnologyAccessoriesContext context, ILogger<PromotionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<Promotion>> GetAllPromotionsAsync()
    {
        return await _context.Promotions
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<Promotion?> GetPromotionByIdAsync(int id)
    {
        return await _context.Promotions.FindAsync(id);
    }

    public async Task<Promotion?> GetPromotionByCodeAsync(string code)
    {
        return await _context.Promotions
            .FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<Promotion> CreatePromotionAsync(Promotion promotion)
    {
        promotion.CreatedAt = DateTime.UtcNow;
        promotion.IsActive = true;

        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Promotion created: {Code}", promotion.Code);
        return promotion;
    }

    public async Task<bool> UpdatePromotionAsync(Promotion promotion)
    {
        try
        {
            var existingPromotion = await _context.Promotions.FindAsync(promotion.PromotionId);
            if (existingPromotion == null)
            {
                return false;
            }

            existingPromotion.Name = promotion.Name;
            existingPromotion.Description = promotion.Description;
            existingPromotion.DiscountAmount = promotion.DiscountAmount;
            existingPromotion.DiscountPercentage = promotion.DiscountPercentage;
            existingPromotion.MinimumOrderAmount = promotion.MinimumOrderAmount;
            existingPromotion.MaxUsageCount = promotion.MaxUsageCount;
            existingPromotion.StartDate = promotion.StartDate;
            existingPromotion.EndDate = promotion.EndDate;
            existingPromotion.IsActive = promotion.IsActive;
            existingPromotion.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Promotion updated: {Code}", promotion.Code);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating promotion: {PromotionId}", promotion.PromotionId);
            return false;
        }
    }

    public async Task<bool> DeletePromotionAsync(int id)
    {
        try
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return false;
            }

            _context.Promotions.Remove(promotion);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Promotion deleted: {Code}", promotion.Code);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting promotion: {PromotionId}", id);
            return false;
        }
    }

    public async Task<bool> ValidatePromotionAsync(string code, decimal orderAmount)
    {
        try
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == code && p.IsActive);

            if (promotion == null)
            {
                return false;
            }

            // Check if promotion is within valid date range
            if (promotion.StartDate > DateTime.UtcNow || promotion.EndDate < DateTime.UtcNow)
            {
                return false;
            }

            // Check minimum order amount
            if (orderAmount < promotion.MinimumOrderAmount)
            {
                return false;
            }

            // Check usage limit
            if (promotion.UsedCount >= promotion.MaxUsageCount)
            {
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating promotion: {Code}", code);
            return false;
        }
    }

    public async Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount)
    {
        try
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == code && p.IsActive);

            if (promotion == null)
            {
                return 0;
            }

            if (!await ValidatePromotionAsync(code, orderAmount))
            {
                return 0;
            }

            decimal discountAmount = 0;

            if (promotion.DiscountPercentage > 0)
            {
                discountAmount = orderAmount * (promotion.DiscountPercentage / 100);
            }
            else
            {
                discountAmount = promotion.DiscountAmount;
            }

            // Ensure discount doesn't exceed order amount
            if (discountAmount > orderAmount)
            {
                discountAmount = orderAmount;
            }

            return discountAmount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating discount for promotion: {Code}", code);
            return 0;
        }
    }

    public async Task<bool> UsePromotionAsync(string code)
    {
        try
        {
            var promotion = await _context.Promotions
                .FirstOrDefaultAsync(p => p.Code == code && p.IsActive);

            if (promotion == null)
            {
                return false;
            }

            promotion.UsedCount++;
            promotion.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Promotion used: {Code}, UsedCount: {UsedCount}", promotion.Code, promotion.UsedCount);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error using promotion: {Code}", code);
            return false;
        }
    }

    public async Task<List<Promotion>> GetActivePromotionsAsync()
    {
        return await _context.Promotions
            .Where(p => p.IsActive && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Promotion>> GetExpiredPromotionsAsync()
    {
        return await _context.Promotions
            .Where(p => p.EndDate < DateTime.UtcNow)
            .OrderByDescending(p => p.EndDate)
            .ToListAsync();
    }

    public async Task<List<Promotion>> GetUpcomingPromotionsAsync()
    {
        return await _context.Promotions
            .Where(p => p.StartDate > DateTime.UtcNow)
            .OrderBy(p => p.StartDate)
            .ToListAsync();
    }

    public async Task<bool> TogglePromotionStatusAsync(int id)
    {
        try
        {
            var promotion = await _context.Promotions.FindAsync(id);
            if (promotion == null)
            {
                return false;
            }

            promotion.IsActive = !promotion.IsActive;
            promotion.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Promotion status toggled: {Code} -> {IsActive}", promotion.Code, promotion.IsActive);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling promotion status: {PromotionId}", id);
            return false;
        }
    }

    public async Task<int> GetTotalPromotionsCountAsync()
    {
        return await _context.Promotions.CountAsync();
    }

    public async Task<int> GetActivePromotionsCountAsync()
    {
        return await _context.Promotions
            .CountAsync(p => p.IsActive && p.StartDate <= DateTime.UtcNow && p.EndDate >= DateTime.UtcNow);
    }

    public async Task<decimal> GetTotalDiscountUsedAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            // This would need to be calculated from order history
            // For now, return 0 as we don't have this data in the current schema
            return 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating total discount used");
            return 0;
        }
    }

    public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
    {
        var query = _context.Promotions.AsQueryable();
        
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.PromotionId != excludeId.Value);
        }

        return !await query.AnyAsync(p => p.Code == code);
    }
}
