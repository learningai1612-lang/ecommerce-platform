using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payment.Application.Common.Interfaces;
using Payment.Application.Payments.Commands.ProcessPayment;
using Payment.Domain.Entities;
using Payment.Domain.Events;

namespace Payment.Application.Payments.EventHandlers;

public class OrderCreatedEventHandler : INotificationHandler<OrderCreatedEvent>
{
    private readonly IPaymentDbContext _context;
    private readonly IMediator _mediator;
    private readonly ILogger<OrderCreatedEventHandler> _logger;

    public OrderCreatedEventHandler(
        IPaymentDbContext context,
        IMediator mediator,
        ILogger<OrderCreatedEventHandler> logger)
    {
        _context = context;
        _mediator = mediator;
        _logger = logger;
    }

    public async Task Handle(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        var eventId = $"OrderCreated_{notification.OrderId}";

        _logger.LogInformation(
            "Received OrderCreatedEvent for order {OrderId}. Event ID: {EventId}",
            notification.OrderId,
            eventId);

        // Check if event already processed (idempotency)
        var alreadyProcessed = await _context.ProcessedEvents
            .AnyAsync(e => e.EventId == eventId, cancellationToken);

        if (alreadyProcessed)
        {
            _logger.LogWarning(
                "Event {EventId} already processed for order {OrderId}. Skipping to ensure idempotency.",
                eventId,
                notification.OrderId);
            return;
        }

        try
        {
            _logger.LogInformation(
                "Processing payment for order {OrderId}. Amount: {Amount:C}",
                notification.OrderId,
                notification.TotalAmount);

            // Process payment
            var command = new ProcessPaymentCommand
            {
                OrderId = notification.OrderId,
                Amount = notification.TotalAmount,
                Currency = "USD"
            };

            var paymentId = await _mediator.Send(command, cancellationToken);

            // Mark event as processed
            var processedEvent = new ProcessedEvent
            {
                EventId = eventId,
                EventType = nameof(OrderCreatedEvent),
                ProcessedAt = DateTime.UtcNow,
                Payload = System.Text.Json.JsonSerializer.Serialize(notification)
            };

            _context.ProcessedEvents.Add(processedEvent);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "OrderCreatedEvent processed successfully for order {OrderId}. Payment ID: {PaymentId}",
                notification.OrderId,
                paymentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Error processing OrderCreatedEvent for order {OrderId}",
                notification.OrderId);
            throw;
        }
    }
}
