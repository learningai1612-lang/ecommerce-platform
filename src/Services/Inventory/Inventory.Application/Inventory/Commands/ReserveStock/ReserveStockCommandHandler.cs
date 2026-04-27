using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Inventory.Commands.ReserveStock;

public class ReserveStockCommandHandler : IRequestHandler<ReserveStockCommand, bool>
{
    private readonly IInventoryDbContext _context;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ReserveStockCommandHandler> _logger;

    public ReserveStockCommandHandler(
        IInventoryDbContext context,
        IEventPublisher eventPublisher,
        ILogger<ReserveStockCommandHandler> logger)
    {
        _context = context;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<bool> Handle(ReserveStockCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Attempting to reserve stock for Order {OrderId}. Product: {ProductId}, Quantity: {Quantity}",
            request.OrderId, request.ProductId, request.Quantity);

        // Use retry logic for optimistic concurrency conflicts
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
                        "Stock reservation failed - Product not found: {ProductId} for Order {OrderId}",
                        request.ProductId, request.OrderId);

                    await PublishFailureEvent(request, 0, "Product not found in inventory");
                    return false;
                }

                // Check availability before attempting reservation
                if (inventory.AvailableQuantity < request.Quantity)
                {
                    _logger.LogWarning(
                        "Insufficient stock for Order {OrderId}. Product: {ProductId}, Requested: {Requested}, Available: {Available}",
                        request.OrderId, request.ProductId, request.Quantity, inventory.AvailableQuantity);

                    await PublishFailureEvent(request, inventory.AvailableQuantity, "Insufficient stock");
                    return false;
                }

                // Reserve stock (domain method enforces invariants)
                inventory.ReserveStock(request.Quantity);

                // Save with optimistic concurrency check
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Stock reserved successfully for Order {OrderId}. Product: {ProductId}, Quantity: {Quantity}, Remaining: {Remaining}",
                    request.OrderId, request.ProductId, request.Quantity, inventory.AvailableQuantity);

                // Publish success event
                await _eventPublisher.PublishAsync(new StockReservedEvent
                {
                    EventId = Guid.NewGuid(),
                    OrderId = request.OrderId,
                    ProductId = request.ProductId,
                    Quantity = request.Quantity,
                    RemainingAvailable = inventory.AvailableQuantity,
                    ReservedAt = DateTime.UtcNow
                });

                // Check if reorder needed
                if (inventory.IsLowStock)
                {
                    _logger.LogWarning(
                        "Low stock alert for Product {ProductId}. Available: {Available}, Reorder Level: {ReorderLevel}",
                        request.ProductId, inventory.AvailableQuantity, inventory.ReorderLevel);
                }

                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                retryCount++;
                _logger.LogWarning(ex,
                    "Concurrency conflict while reserving stock for Order {OrderId}, Product {ProductId}. Retry {Retry}/{MaxRetries}",
                    request.OrderId, request.ProductId, retryCount, maxRetries);

                if (retryCount >= maxRetries)
                {
                    _logger.LogError(
                        "Failed to reserve stock after {MaxRetries} retries due to concurrency conflicts. Order {OrderId}, Product {ProductId}",
                        maxRetries, request.OrderId, request.ProductId);

                    await PublishFailureEvent(request, 0, "Concurrency conflict - too many retries");
                    return false;
                }

                // Refresh context for retry
                await Task.Delay(TimeSpan.FromMilliseconds(100 * retryCount), cancellationToken);
            }
        }

        return false;
    }

    private async Task PublishFailureEvent(ReserveStockCommand request, int availableQuantity, string reason)
    {
        await _eventPublisher.PublishAsync(new StockReservationFailedEvent
        {
            EventId = Guid.NewGuid(),
            OrderId = request.OrderId,
            ProductId = request.ProductId,
            RequestedQuantity = request.Quantity,
            AvailableQuantity = availableQuantity,
            Reason = reason,
            FailedAt = DateTime.UtcNow
        });
    }
}
