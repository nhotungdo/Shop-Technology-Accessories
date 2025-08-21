using Microsoft.AspNetCore.Mvc;
using ShopTechnology.Services;
using ShopTechnology.DTOs;

namespace ShopTechnology.Controllers.Api;

[ApiController]
[Route("api/[controller]")]
public class CartController : ControllerBase
{
    private readonly ICartService _cartService;

    public CartController(ICartService cartService)
    {
        _cartService = cartService;
    }

    [HttpGet]
    public async Task<ActionResult<CartDTO>> GetCart([FromQuery] Guid userId)
    {
        try
        {
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            return cart == null 
                ? NotFound(new { error = "Cart not found." })
                : Ok(cart);
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while retrieving the cart." });
        }
    }

    [HttpPost("add")]
    public async Task<ActionResult> AddToCart([FromBody] AddToCartRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var addToCartDto = new AddToCartDTO
            {
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            var result = await _cartService.AddToCartAsync(request.UserId, addToCartDto);
            return result 
                ? Ok(new { message = "Item added to cart successfully." })
                : BadRequest(new { error = "Failed to add item to cart. Product may be out of stock." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while adding item to cart." });
        }
    }

    [HttpPut("update/{cartItemId}")]
    public async Task<ActionResult> UpdateCartItem(int cartItemId, [FromBody] UpdateCartItemRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _cartService.UpdateCartItemQuantityAsync(cartItemId, request.Quantity);
            return result 
                ? Ok(new { message = "Cart item updated successfully." })
                : BadRequest(new { error = "Failed to update cart item. Product may be out of stock." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while updating cart item." });
        }
    }

    [HttpDelete("remove/{cartItemId}")]
    public async Task<ActionResult> RemoveFromCart(int cartItemId)
    {
        try
        {
            var result = await _cartService.RemoveFromCartAsync(cartItemId);
            return result 
                ? Ok(new { message = "Item removed from cart successfully." })
                : NotFound(new { error = "Cart item not found." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while removing item from cart." });
        }
    }

    [HttpDelete("clear")]
    public async Task<ActionResult> ClearCart([FromQuery] Guid userId)
    {
        try
        {
            var result = await _cartService.ClearCartAsync(userId);
            return result 
                ? Ok(new { message = "Cart cleared successfully." })
                : NotFound(new { error = "Cart not found." });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while clearing cart." });
        }
    }

    [HttpGet("count")]
    public async Task<ActionResult<object>> GetCartItemCount([FromQuery] Guid userId)
    {
        try
        {
            var count = await _cartService.GetCartItemCountAsync(userId);
            return Ok(new { count });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while getting cart count." });
        }
    }

    [HttpGet("total")]
    public async Task<ActionResult<object>> GetCartTotal([FromQuery] Guid userId)
    {
        try
        {
            var total = await _cartService.GetCartTotalAsync(userId);
            return Ok(new { total = Math.Round(total, 2) });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while getting cart total." });
        }
    }

    [HttpGet("summary")]
    public async Task<ActionResult<object>> GetCartSummary([FromQuery] Guid userId)
    {
        try
        {
            var cart = await _cartService.GetCartByUserIdAsync(userId);
            if (cart == null)
            {
                return Ok(new { itemCount = 0, total = 0 });
            }

            return Ok(new
            {
                itemCount = cart.Items.Sum(i => i.Quantity),
                total = Math.Round(cart.Items.Sum(i => i.Price * i.Quantity), 2)
            });
        }
        catch (Exception)
        {
            return StatusCode(500, new { error = "An error occurred while getting cart summary." });
        }
    }
}

public class AddToCartRequest
{
    public Guid UserId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; } = 1;
}

public class UpdateCartItemRequest
{
    public int Quantity { get; set; }
}
