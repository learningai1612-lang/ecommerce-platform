using MediatR;
using Microsoft.Extensions.Logging;
using Payment.Application.Common.Interfaces;
using Payment.Domain.Entities;
using Payment.Domain.Events;

namespace Payment.Application.Payments.Commands.ProcessPayment;

public class ProcessPaymentCommandHandler : IRequestHandler<ProcessPaymentCommand, Guid>
{
    private readonly IPaymentDbContext _context;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IEventPublisher _eventPublisher;
    private readonly ILogger<ProcessPaymentCommandHandler> _logger;

    public ProcessPaymentCommandHandler(
        IPaymentDbContext context,
        IPaymentGateway paymentGateway,
        IEventPublisher eventPublisher,
        ILogger<ProcessPaymentCommandHandler> logger)
    {
        _context = context;
        _paymentGateway = paymentGateway;
        _eventPublisher = eventPublisher;
        _logger = logger;
    }

    public async Task<Guid> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Processing payment for order {OrderId}. Amount: {Amount:C} {Currency}",
            request.OrderId,
            request.Amount,
            request.Currency);

        // Create payment record
        var payment = new PaymentTransaction(request.OrderId, request.Amount, request.Currency);
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment {PaymentId} created in Pending status", payment.Id);

        try
        {
            // Mark as processing
            payment.MarkAsProcessing();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Calling external payment gateway for payment {PaymentId}", payment.Id);

            // Call external payment gateway (simulated)
            var result = await _paymentGateway.ProcessPaymentAsync(
                request.Amount,
                request.Currency,
                request.OrderId);

            if (result.Success)
            {
                // Payment succeeded
                payment.MarkAsCompleted(result.TransactionId!);
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Payment {PaymentId} completed successfully. Transaction ID: {TransactionId}",
                    payment.Id,
                    result.TransactionId);

                // Publish success event
                var successEvent = new PaymentSucceededEvent
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    TransactionId = payment.TransactionId!,
                    ProcessedAt = payment.ProcessedAt!.Value
                };

                await _eventPublisher.PublishAsync(successEvent, cancellationToken);

                _logger.LogInformation(
                    "PaymentSucceededEvent published for payment {PaymentId}, order {OrderId}",
                    payment.Id,
                    payment.OrderId);
            }
            else
            {
                // Payment failed
                payment.MarkAsFailed(result.ErrorMessage ?? "Payment gateway error");
                await _context.SaveChangesAsync(cancellationToken);

                _logger.LogWarning(
                    "Payment {PaymentId} failed. Reason: {Reason}",
                    payment.Id,
                    result.ErrorMessage);

                // Publish failure event
                var failureEvent = new PaymentFailedEvent
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    Amount = payment.Amount,
                    Currency = payment.Currency,
                    Reason = payment.FailureReason!,
                    FailedAt = payment.ProcessedAt!.Value
                };

                await _eventPublisher.PublishAsync(failureEvent, cancellationToken);

                _logger.LogInformation(
                    "PaymentFailedEvent published for payment {PaymentId}, order {OrderId}",
                    payment.Id,
                    payment.OrderId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing payment {PaymentId} for order {OrderId}", payment.Id, request.OrderId);
            
            // Mark as failed
            payment.MarkAsFailed($"System error: {ex.Message}");
            await _context.SaveChangesAsync(cancellationToken);

            throw;
        }

        return payment.Id;
    }
}
