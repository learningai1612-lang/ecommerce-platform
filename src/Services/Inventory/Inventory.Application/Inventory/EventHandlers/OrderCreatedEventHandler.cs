using Inventory.Application.Inventory.Commands.ReserveStock;
using Inventory.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Inventory.EventHandlers;

public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(
        IMediator mediator,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing OrderCreatedEvent {EventId} for Order {OrderId} with {ItemCount} items",
            notification.EventId, notification.OrderId, notification.Items.Count);

        // Reserve stock for each order item
        foreach (var item in notification.Items)
        {
            var command = new ReserveStockCommand
            {
                OrderId = notification.OrderId,
                ProductId = item.ProductId,
                Quantity = item.Quantity
            };

            var success = await _mediator.Send(command, cancellationToken);

            if (!success)
            {
                _logger.LogError(
                    "Failed to reserve stock for Order {OrderId}, Product {ProductId}. Reservation process halted.",
                    notification.OrderId, item.ProductId);

                // In a real system, this would trigger a compensating transaction
                // to release any already-reserved stock for this order
                break;
            }
        }

        _logger.LogInformation(
            "Completed stock reservation processing for Order {OrderId}",
            notification.OrderId);
    }
}
