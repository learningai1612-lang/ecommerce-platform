using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Inventory.Commands.ReleaseStock;

public class ReleaseStockCommandHandler : IRequestHandler<ReleaseStockCommand, bool>
{
    private readonly IInventoryDbContext _context;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ReleaseStockCommandHandler> _logger;

    public ReleaseStockCommandHandler(
        IInventoryDbContext context,
        IEventPublisher eventPublisher,
        ILogger<ReleaseStockCommandHandler> logger)
    {
        _context = context;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(ReleaseStockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Releasing reserved stock for Order {OrderId}. Product: {ProductId}, Quantity: {Quantity}, Reason: {Reason}",
            request.OrderId, request.ProductId, request.Quantity, request.Reason);

        const int maxRetries = 3;
        int retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                var inventory = await _context.InventoryItems
                    .FirstOrDefaultAsync(i => i.ProductId == request.ProductId, cancellationToken);

                if (inventory == null)
                {
                    _logger.LogWarning(
                        "Cannot release stock - Product not found: {ProductId} for Order {OrderId}",
                        request.ProductId, request.OrderId);
                    return false;
                }

                // Release reserved stock back to available
                inventory.ReleaseReservedStock(request.Quantity);

                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Stock released successfully for Order {OrderId}. Product: {ProductId}, Quantity: {Quantity}, New Available: {Available}",
                    request.OrderId, request.ProductId, request.Quantity, inventory.AvailableQuantity);

                // Publish event
                await _eventPublisher.PublishAsync(new StockReleasedEvent
                {
                    EventId = Guid.NewGuid(),
                    OrderId = request.OrderId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    Reason = request.Reason,
                    ReleasedAt = DateTime.UtcNow
                });

                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                retryCount++;
                _logger.LogWarning(ex,
                    "Concurrency conflict while releasing stock for Order {OrderId}. Retry {Retry}/{MaxRetries}",
                    request.OrderId, retryCount, maxRetries);

                if (retryCount >= maxRetries)
                {
                    _logger.LogError(
                        "Failed to release stock after {MaxRetries} retries. Order {OrderId}, Product {ProductId}",
                        maxRetries, request.OrderId, request.ProductId);
                    return false;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(100 * retryCount), cancellationToken);
            }
        }

        return false;
    }
}
