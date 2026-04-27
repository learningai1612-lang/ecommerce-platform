using MediatR;
using Microsoft.Extensions.Logging;
using Cart.Application.Common.Interfaces;

namespace Cart.Application.Carts.Commands.ClearCart;

public class ClearCartCommandHandler : IRequestHandler<ClearCartCommand, Unit>
{
    private readonly ICartRepository _repository;
    private readonly ILogger<ClearCartCommandHandler> _logger;

    public ClearCartCommandHandler(
        ICartRepository repository,
        ILogger<ClearCartCommandHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<Unit> Handle(ClearCartCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Clearing cart for user {UserId}", request.UserId);

        // Delete cart from Redis (idempotent operation)
        await _repository.DeleteAsync(request.UserId, cancellationToken);

        _logger.LogInformation("Cart cleared successfully for user {UserId}", request.UserId);

        return Unit.Value;
    }
}
