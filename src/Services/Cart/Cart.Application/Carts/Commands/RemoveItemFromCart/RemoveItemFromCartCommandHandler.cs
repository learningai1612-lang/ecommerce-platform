using MediatR;
using Microsoft.Extensions.Logging;
using Cart.Application.Common.Exceptions;
using Cart.Application.Common.Interfaces;

namespace Cart.Application.Carts.Commands.RemoveItemFromCart;

public class RemoveItemFromCartCommandHandler : IRequestHandler<RemoveItemFromCartCommand, Unit>
{
    private readonly ICartRepository _repository;
    private readonly ILogger<RemoveItemFromCartCommandHandler> _logger;

    public RemoveItemFromCartCommandHandler(
        ICartRepository repository,
        ILogger<RemoveItemFromCartCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Unit> Handle(RemoveItemFromCartCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Removing item {ItemId} from cart for user {UserId}",
            request.ItemId,
            request.UserId);

        var cart = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (cart == null)
        {
            _logger.LogWarning("Cart not found for user {UserId}", request.UserId);
            throw new NotFoundException("ShoppingCart", request.UserId);
        }

        try
        {
            cart.RemoveItem(request.ItemId);
            await _repository.SaveAsync(cart, cancellationToken);

            _logger.LogInformation(
                "Item {ItemId} removed from cart {CartId}. Remaining items: {TotalItems}, Total amount: {TotalAmount:C}",
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
