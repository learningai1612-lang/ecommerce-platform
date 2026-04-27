using Inventory.Application.Inventory.Commands.ReleaseStock;
using Inventory.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Inventory.EventHandlers;

public class OrderCancelledEventHandler : INotificationHandler<OrderCancelledEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<OrderCancelledEventHandler> _logger;

    public OrderCancelledEventHandler(
        IMediator mediator,
        ILogger<OrderCancelledEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(OrderCancelledEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing OrderCancelledEvent {EventId} for Order {OrderId}. Releasing reserved stock. Reason: {Reason}",
            notification.EventId, notification.OrderId, notification.Reason);

        // In a real system, look up reserved items and release them
        
        _logger.LogWarning(
            "Order {OrderId} cancelled. Reserved stock should be released. Reason: {Reason}",
            notification.OrderId, notification.Reason);

        // In production with order-item tracking:
        // foreach (var item in orderItems)
        // {
        //     var command = new ReleaseStockCommand
        //     {
        //         OrderId = notification.OrderId,
        //         ProductId = item.ProductId,
        //         Quantity = item.Quantity,
        //         Reason = $"Order cancelled: {notification.Reason}"
        //     };
        //     await _mediator.Send(command, cancellationToken);
        // }
    }
}
