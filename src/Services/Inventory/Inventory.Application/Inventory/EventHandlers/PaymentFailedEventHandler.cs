using Inventory.Application.Inventory.Commands.ReleaseStock;
using Inventory.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Inventory.EventHandlers;

public class PaymentFailedEventHandler : INotificationHandler<PaymentFailedEvent>
{
    private readonly IMediator _mediator;
    private readonly ILogger<PaymentFailedEventHandler> _logger;

    public PaymentFailedEventHandler(
        IMediator mediator,
        ILogger<PaymentFailedEventHandler> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(PaymentFailedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing PaymentFailedEvent {EventId} for Order {OrderId}. Releasing reserved stock. Reason: {Reason}",
            notification.EventId, notification.OrderId, notification.Reason);

        // In a real system, we would:
        // 1. Look up which products were reserved for this order
        // 2. Release all reserved stock back to available

        // For demonstration, we log the intent
        // In production, you'd have a reservation tracking table to know which products to release
        
        _logger.LogWarning(
            "Payment failed for Order {OrderId}. Reserved stock should be released. Reason: {Reason}",
            notification.OrderId, notification.Reason);

        // Example of how you'd release stock if you had the product list:
        // foreach (var item in orderItems)
        // {
        //     var command = new ReleaseStockCommand
        //     {
        //         OrderId = notification.OrderId,
        //         ProductId = item.ProductId,
        //         Quantity = item.Quantity,
        //         Reason = $"Payment failed: {notification.Reason}"
        //     };
        //     await _mediator.Send(command, cancellationToken);
        // }
    }
}
