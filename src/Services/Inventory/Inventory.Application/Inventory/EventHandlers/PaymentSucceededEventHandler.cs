using Inventory.Application.Common.Interfaces;
using Inventory.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Inventory.Application.Inventory.EventHandlers;

public class PaymentSucceededEventHandler : INotificationHandler<PaymentSucceededEvent>
{
    private readonly IInventoryDbContext _context;
    private readonly ILogger<PaymentSucceededEventHandler> _logger;

    public PaymentSucceededEventHandler(
        IInventoryDbContext context,
        ILogger<PaymentSucceededEventHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task Handle(PaymentSucceededEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing PaymentSucceededEvent {EventId} for Order {OrderId}. Committing reserved stock.",
            notification.EventId, notification.OrderId);

        // In a real system, we would:
        // 1. Look up which products were reserved for this order
        // 2. Commit the reserved stock (reduce ReservedQuantity permanently)
        
        // For now, we'll log that payment succeeded and stock should be committed
        _logger.LogInformation(
            "Payment succeeded for Order {OrderId}. Reserved stock will be committed (shipped).",
            notification.OrderId);

        // This is where you'd call inventory.CommitReservedStock() for each item
        // once you have order-product tracking in place
    }
}
