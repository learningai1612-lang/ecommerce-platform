using Microsoft.Extensions.Logging;
using Payment.Application.Common.Interfaces;

namespace Payment.Infrastructure.Services;

public class SimulatedPaymentGateway : IPaymentGateway
{
    private readonly ILogger<SimulatedPaymentGateway> _logger;
    private static readonly Random _random = new();

    public SimulatedPaymentGateway(ILogger<SimulatedPaymentGateway> logger)
    {
        _logger = logger;
    }

    public async Task<PaymentGatewayResult> ProcessPaymentAsync(decimal amount, string currency, Guid orderId)
    {
        _logger.LogInformation(
            "Simulated payment gateway processing payment. Order: {OrderId}, Amount: {Amount:C} {Currency}",
            orderId,
            amount,
            currency);

        // Simulate network delay (100-500ms)
        var delay = _random.Next(100, 500);
        await Task.Delay(delay);

        // Simulate 80% success rate
        var success = _random.Next(100) < 80;

        if (success)
        {
            var transactionId = $"TXN_{Guid.NewGuid().ToString("N")[..12].ToUpper()}";
            
            _logger.LogInformation(
                "Payment gateway SUCCESS. Transaction ID: {TransactionId}, Order: {OrderId}",
                transactionId,
                orderId);

            return new PaymentGatewayResult
            {
                Success = true,
                TransactionId = transactionId
            };
        }
        else
        {
            var errorMessages = new[]
            {
                "Insufficient funds",
                "Card declined",
                "Invalid card number",
                "Card expired",
                "Gateway timeout"
            };

            var errorMessage = errorMessages[_random.Next(errorMessages.Length)];

            _logger.LogWarning(
                "Payment gateway FAILURE. Reason: {Reason}, Order: {OrderId}",
                errorMessage,
                orderId);

            return new PaymentGatewayResult
            {
                Success = false,
                ErrorMessage = errorMessage
            };
        }
    }
}
