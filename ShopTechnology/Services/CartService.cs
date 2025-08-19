using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ShopTechnology.DTOs;
using ShopTechnology.Models;

namespace ShopTechnology.Services;

public class CartService : ICartService
{
    private readonly ShopTechnologyAccessoriesContext _context;
    private readonly IMapper _mapper;
    private readonly IProductService _productService;

    public CartService(ShopTechnologyAccessoriesContext context, IMapper mapper, IProductService productService)
    {
        _context = context;
        _mapper = mapper;
        _productService = productService;
    }

    public async Task<CartDTO?> GetCartByUserIdAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.User)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.ProductImages)
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                    .ThenInclude(p => p.Category)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return null;
        }

        var cartDto = _mapper.Map<CartDTO>(cart);

        // Set additional properties for cart items
        foreach (var item in cartDto.CartItems)
        {
            var product = cart.CartItems.First(ci => ci.ProductId == item.ProductId).Product;
            item.IsInStock = product.StockQuantity > 0;
            item.AvailableStock = product.StockQuantity;
        }

        return cartDto;
    }

    public async Task<CartDTO> CreateCartAsync(Guid userId)
    {
        var cart = new Cart
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Carts.Add(cart);
        await _context.SaveChangesAsync();

        return await GetCartByUserIdAsync(userId) ?? throw new InvalidOperationException("Failed to create cart");
    }

    public async Task<bool> ClearCartAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return false;
        }

        _context.CartItems.RemoveRange(cart.CartItems);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> AddToCartAsync(Guid userId, AddToCartDTO addToCartDto)
    {
        // Validate product and quantity
        if (!await ValidateCartItemAsync(addToCartDto.ProductId, addToCartDto.Quantity))
        {
            return false;
        }

        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();
        }

        // Check if product already exists in cart
        var existingItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == addToCartDto.ProductId);
        if (existingItem != null)
        {
            existingItem.Quantity += addToCartDto.Quantity;
        }
        else
        {
            var cartItem = new CartItem
            {
                CartId = cart.CartId,
                ProductId = addToCartDto.ProductId,
                Quantity = addToCartDto.Quantity
            };
            _context.CartItems.Add(cartItem);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> UpdateCartItemQuantityAsync(int cartItemId, int quantity)
    {
        if (quantity <= 0)
        {
            return await RemoveFromCartAsync(cartItemId);
        }

        var cartItem = await _context.CartItems
            .Include(ci => ci.Product)
            .FirstOrDefaultAsync(ci => ci.CartItemId == cartItemId);

        if (cartItem == null)
        {
            return false;
        }

        // Check if requested quantity is available
        if (quantity > cartItem.Product.StockQuantity)
        {
            return false;
        }

        cartItem.Quantity = quantity;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveFromCartAsync(int cartItemId)
    {
        var cartItem = await _context.CartItems.FindAsync(cartItemId);
        if (cartItem == null)
        {
            return false;
        }

        _context.CartItems.Remove(cartItem);
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> RemoveAllFromCartAsync(Guid userId)
    {
        return await ClearCartAsync(userId);
    }

    public async Task<int> GetCartItemCountAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        return cart?.CartItems.Sum(ci => ci.Quantity) ?? 0;
    }

    public async Task<decimal> GetCartTotalAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return 0;
        }

        return cart.CartItems.Sum(ci => ci.Quantity * ci.Product.Price);
    }

    public async Task<bool> IsCartEmptyAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        return cart?.CartItems.Any() != true;
    }

    public async Task<bool> ValidateCartItemAsync(int productId, int quantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
        {
            return false;
        }

        return product.StockQuantity >= quantity && quantity > 0;
    }

    public async Task<List<CartItemDTO>> GetInvalidCartItemsAsync(Guid userId)
    {
        var cart = await _context.Carts
            .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null)
        {
            return new List<CartItemDTO>();
        }

        var invalidItems = new List<CartItemDTO>();

        foreach (var cartItem in cart.CartItems)
        {
            if (cartItem.Quantity > cartItem.Product.StockQuantity || cartItem.Product.StockQuantity == 0)
            {
                var cartItemDto = _mapper.Map<CartItemDTO>(cartItem);
                cartItemDto.IsInStock = false;
                cartItemDto.AvailableStock = cartItem.Product.StockQuantity;
                invalidItems.Add(cartItemDto);
            }
        }

        return invalidItems;
    }
}
