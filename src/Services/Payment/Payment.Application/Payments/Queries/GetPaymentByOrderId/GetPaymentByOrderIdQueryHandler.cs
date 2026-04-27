using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Payment.Application.Common.DTOs;
using Payment.Application.Common.Interfaces;

namespace Payment.Application.Payments.Queries.GetPaymentByOrderId;

public class GetPaymentByOrderIdQueryHandler : IRequestHandler<GetPaymentByOrderIdQuery, PaymentResponseDto?>
{
    private readonly IPaymentDbContext _context;
    private readonly ILogger<GetPaymentByOrderIdQueryHandler> _logger;

    public GetPaymentByOrderIdQueryHandler(
        IPaymentDbContext context,
        ILogger<GetPaymentByOrderIdQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<PaymentResponseDto?> Handle(GetPaymentByOrderIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving payment for order {OrderId}", request.OrderId);

        var payment = await _context.Payments
            .Where(p => p.OrderId == request.OrderId)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (payment == null)
        {
            _logger.LogInformation("No payment found for order {OrderId}", request.OrderId);
            return null;
        }

        var response = new PaymentResponseDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            TransactionId = payment.TransactionId,
            FailureReason = payment.FailureReason,
            ProcessedAt = payment.ProcessedAt,
            CreatedAt = payment.CreatedAt
        };

        _logger.LogInformation(
            "Payment retrieved for order {OrderId}. Status: {Status}, Amount: {Amount:C}",
            request.OrderId,
            payment.Status,
            payment.Amount);

        return response;
    }
}
