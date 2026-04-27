using MediatR;
using Microsoft.Extensions.Logging;
using Cart.Application.Common.Exceptions;
using Cart.Application.Common.Interfaces;

namespace Cart.Application.Carts.Commands.UpdateCartItemQuantity;

public class UpdateCartItemQuantityCommandHandler : IRequestHandler<UpdateCartItemQuantityCommand, Unit>
{
    private readonly ICartRepository _repository;
    private readonly ILogger<UpdateCartItemQuantityCommandHandler> _logger;

    public UpdateCartItemQuantityCommandHandler(
        ICartRepository repository,
        ILogger<UpdateCartItemQuantityCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Unit> Handle(UpdateCartItemQuantityCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Updating quantity for item {ItemId} in cart for user {UserId}. New quantity: {Quantity}",
            request.ItemId,
            request.UserId,
            request.Quantity);

        var cart = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart == null)
        {
            _logger.LogWarning("Cart not found for user {UserId}", request.UserId);
            throw new NotFoundException("ShoppingCart", request.UserId);
        }

        try
        {
            cart.UpdateItemQuantity(request.ItemId, request.Quantity);
            await _repository.SaveAsync(cart, cancellationToken);

            _logger.LogInformation(
                "Item {ItemId} quantity updated in cart {CartId}. Total items: {TotalItems}, Total amount: {TotalAmount:C}",
                request.ItemId,
                cart.Id,
                cart.TotalItems,
                cart.TotalAmount);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Item {ItemId} not found in cart for user {UserId}", request.ItemId, request.UserId);
            throw new NotFoundException($"Cart item {request.ItemId} not found in cart.");
        }

        return Unit.Value;
    }
}
