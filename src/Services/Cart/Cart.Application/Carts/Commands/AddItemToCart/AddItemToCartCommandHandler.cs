using MediatR;
using Microsoft.Extensions.Logging;
using Cart.Application.Common.Interfaces;
using Cart.Domain.Entities;

namespace Cart.Application.Carts.Commands.AddItemToCart;

public class AddItemToCartCommandHandler : IRequestHandler<AddItemToCartCommand, Guid>
{
    private readonly ICartRepository _repository;
    private readonly ILogger<AddItemToCartCommandHandler> _logger;

    public AddItemToCartCommandHandler(
        ICartRepository repository,
        ILogger<AddItemToCartCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Guid> Handle(AddItemToCartCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding item to cart for user {UserId}. Product: {ProductId} ({ProductName}), Quantity: {Quantity}, Price: {Price:C}",
            request.UserId,
            request.ProductId,
            request.ProductName,
            request.Quantity,
            request.Price);

        // Get or create cart
        var cart = await _repository.GetByUserIdAsync(request.UserId, cancellationToken);
        
        if (cart == null)
        {
            _logger.LogInformation("Creating new cart for user {UserId}", request.UserId);
            cart = new ShoppingCart(request.UserId);
        }

        // Add item (or increase quantity if already exists)
        var itemCountBefore = cart.Items.Count;
        cart.AddItem(request.ProductId, request.ProductName, request.Price, request.Quantity);
        var itemCountAfter = cart.Items.Count;

        // Save cart
        await _repository.SaveAsync(cart, cancellationToken);

        if (itemCountAfter > itemCountBefore)
        {
            _logger.LogInformation(
                "New item added to cart {CartId} for user {UserId}. Product: {ProductName} ({ProductId}), Unique items: {UniqueItems}, Total items: {TotalItems}, Total amount: {TotalAmount:C}",
                cart.Id,
                cart.UserId,
                request.ProductName,
                request.ProductId,
                cart.Items.Count,
                cart.TotalItems,
                cart.TotalAmount);
        }
        else
        {
            _logger.LogInformation(
                "Item quantity increased in cart {CartId} for user {UserId}. Product: {ProductName} ({ProductId}), Total items: {TotalItems}, Total amount: {TotalAmount:C}",
                cart.Id,
                cart.UserId,
                request.ProductName,
                request.ProductId,
                cart.TotalItems,
                cart.TotalAmount);
        }

        return cart.Id;
    }
}
