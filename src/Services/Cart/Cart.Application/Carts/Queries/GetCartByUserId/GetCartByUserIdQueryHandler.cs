using MediatR;
using Microsoft.Extensions.Logging;
using Cart.Application.Common.DTOs;
using Cart.Application.Common.Interfaces;

namespace Cart.Application.Carts.Queries.GetCartByUserId;

public class GetCartByUserIdQueryHandler : IRequestHandler<GetCartByUserIdQuery, CartResponseDto?>
{
    private readonly ICartRepository _repository;
    private readonly ILogger<GetCartByUserIdQueryHandler> _logger;

    public GetCartByUserIdQueryHandler(
        ICartRepository repository,
        ILogger<GetCartByUserIdQueryHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<CartResponseDto?> Handle(GetCartByUserIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving cart for user {UserId}", request.UserId);

        var cart = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart == null)
        {
            _logger.LogInformation("No cart found for user {UserId}", request.UserId);
            return null;
        }

        var response = new CartResponseDto
        {
            Id = cart.Id,
            UserId = cart.UserId,
            TotalAmount = cart.TotalAmount,
            TotalItems = cart.TotalItems,
            CreatedAt = cart.CreatedAt,
            UpdatedAt = cart.UpdatedAt,
            Items = cart.Items.Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Price = item.Price,
                Quantity = item.Quantity,
                Subtotal = item.Subtotal
            }).ToList()
        };

        _logger.LogInformation(
            "Cart {CartId} retrieved for user {UserId}. Items: {ItemCount}, Total: {TotalAmount:C}",
            cart.Id,
            cart.UserId,
            cart.Items.Count,
            cart.TotalAmount);

        return response;
    }
}
