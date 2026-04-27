using MediatR;
using Microsoft.AspNetCore.Mvc;
using Cart.Application.Carts.Commands.AddItemToCart;
using Cart.Application.Carts.Commands.RemoveItemFromCart;
using Cart.Application.Carts.Commands.UpdateCartItemQuantity;
using Cart.Application.Carts.Commands.ClearCart;
using Cart.Application.Carts.Queries.GetCartByUserId;

namespace Cart.API.Controllers;

[ApiController]
[Route("api/cart")]
public class CartsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CartsController> _logger;

    public CartsController(IMediator mediator, ILogger<CartsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Get cart by user ID
    /// </summary>
    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCart(Guid userId)
    {
        _logger.LogInformation("API: Retrieving cart for user {UserId}", userId);

        var query = new GetCartByUserIdQuery { UserId = userId };
        var cart = await _mediator.Send(query);

        if (cart == null)
        {
            _logger.LogInformation("API: No cart found for user {UserId}", userId);
            return NotFound(new { message = $"Cart not found for user {userId}" });
        }

        _logger.LogInformation(
            "API: Cart retrieved for user {UserId}. Items: {ItemCount}, Total: {TotalAmount:C}",
            userId,
            cart.Items.Count,
            cart.TotalAmount);

        return Ok(cart);
    }

    /// <summary>
    /// Add item to cart
    /// </summary>
    [HttpPost("items")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> AddItem([FromBody] AddItemToCartCommand command)
    {
        _logger.LogInformation(
            "API: Adding item to cart for user {UserId}. Product: {ProductId} ({ProductName}), Quantity: {Quantity}",
            command.UserId,
            command.ProductId,
            command.ProductName,
            command.Quantity);

        var cartId = await _mediator.Send(command);

        _logger.LogInformation("API: Item added successfully to cart {CartId} for user {UserId}", cartId, command.UserId);

        return Ok(new { cartId, message = "Item added to cart successfully" });
    }

    /// <summary>
    /// Update cart item quantity
    /// </summary>
    [HttpPut("items")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateItemQuantity([FromBody] UpdateCartItemQuantityCommand command)
    {
        _logger.LogInformation(
            "API: Updating item {ItemId} quantity to {NewQuantity} for user {UserId}",
            command.ItemId,
            command.Quantity,
            command.UserId);

        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    [HttpDelete("items/{itemId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveItem(Guid itemId, [FromQuery] Guid userId)
    {
        _logger.LogInformation(
            "API: Removing item {ItemId} from cart for user {UserId}",
            itemId,
            userId);

        var command = new RemoveItemFromCartCommand
        {
            UserId = userId,
            ItemId = itemId
        };

        await _mediator.Send(command);

        return NoContent();
    }

    /// <summary>
    /// Clear cart
    /// </summary>
    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ClearCart(Guid userId)
    {
        _logger.LogInformation("API: Clearing cart for user {UserId}", userId);

        var command = new ClearCartCommand { UserId = userId };
        await _mediator.Send(command);

        return NoContent();
    }
}
