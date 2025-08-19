using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.DTOs;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class PromotionService : IPromotionService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IMapper _mapper;

    public PromotionService(ShopTechnologyAccessoriesContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<PromotionDTO>> GetAllPromotionsAsync()
    {
        var promotions = await _context.Promotions
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return _mapper.Map<List<PromotionDTO>>(promotions);
    }

    public async Task<PromotionDTO?> GetPromotionByIdAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        return _mapper.Map<PromotionDTO>(promotion);
    }

    public async Task<PromotionDTO?> GetPromotionByCodeAsync(string code)
    {
        var promotion = await _context.Promotions
            .FirstOrDefaultAsync(p => p.Code == code);

        return _mapper.Map<PromotionDTO>(promotion);
    }

    public async Task<PromotionDTO> CreatePromotionAsync(CreatePromotionDTO createPromotionDto)
    {
        // Check if code already exists
        if (await _context.Promotions.AnyAsync(p => p.Code == createPromotionDto.Code))
        {
            throw new InvalidOperationException("Promotion code already exists");
        }

        var promotion = _mapper.Map<Promotion>(createPromotionDto);
        promotion.CreatedAt = DateTime.UtcNow;

        _context.Promotions.Add(promotion);
        await _context.SaveChangesAsync();

        return await GetPromotionByIdAsync(promotion.PromotionId) ?? throw new InvalidOperationException("Failed to create promotion");
    }

    public async Task<PromotionDTO> UpdatePromotionAsync(int id, UpdatePromotionDTO updatePromotionDto)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null)
        {
            throw new InvalidOperationException("Promotion not found");
        }

        _mapper.Map(updatePromotionDto, promotion);
        promotion.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return await GetPromotionByIdAsync(id) ?? throw new InvalidOperationException("Failed to update promotion");
    }

    public async Task<bool> DeletePromotionAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null)
        {
            return false;
        }

        _context.Promotions.Remove(promotion);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> ActivatePromotionAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null)
        {
            return false;
        }

        promotion.IsActive = true;
        promotion.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> DeactivatePromotionAsync(int id)
    {
        var promotion = await _context.Promotions.FindAsync(id);
        if (promotion == null)
        {
            return false;
        }

        promotion.IsActive = false;
        promotion.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<decimal> CalculateDiscountAsync(string code, decimal orderAmount)
    {
        var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.Code == code);
        if (promotion == null || !IsPromotionValid(promotion) || orderAmount < promotion.MinimumOrderAmount)
        {
            return 0;
        }

        var discount = promotion.DiscountAmount;
        if (promotion.DiscountPercentage > 0)
        {
            var percentageDiscount = orderAmount * (promotion.DiscountPercentage / 100);
            discount = Math.Max(discount, percentageDiscount);
        }

        return Math.Min(discount, orderAmount); // Discount cannot exceed order amount
    }

    public async Task<bool> ValidatePromotionAsync(string code, decimal orderAmount)
    {
        var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.Code == code);
        if (promotion == null)
        {
            return false;
        }

        return IsPromotionValid(promotion) && orderAmount >= promotion.MinimumOrderAmount;
    }

    public async Task<bool> UsePromotionAsync(string code)
    {
        var promotion = await _context.Promotions.FirstOrDefaultAsync(p => p.Code == code);
        if (promotion == null || !IsPromotionValid(promotion))
        {
            return false;
        }

        promotion.UsedCount++;
        promotion.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<List<PromotionDTO>> GetActivePromotionsAsync()
    {
        var now = DateTime.Now;
        var promotions = await _context.Promotions
            .Where(p => p.IsActive && p.StartDate <= now && p.EndDate >= now && p.UsedCount < p.MaxUsageCount)
            .OrderBy(p => p.StartDate)
            .ToListAsync();

        return _mapper.Map<List<PromotionDTO>>(promotions);
    }

    public async Task<int> GetTotalPromotionsCountAsync()
    {
        return await _context.Promotions.CountAsync();
    }

    private bool IsPromotionValid(Promotion promotion)
    {
        var now = DateTime.Now;
        return promotion.IsActive &&
               promotion.StartDate <= now &&
               promotion.EndDate >= now &&
               promotion.UsedCount < promotion.MaxUsageCount;
    }
}
