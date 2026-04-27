namespace Payment.Application.Common.Interfaces;

public interface IPaymentGateway
{
    /// <summary>
    /// Process payment through external gateway (simulated)
    /// </summary>
    Task<PaymentGatewayResult> ProcessPaymentAsync(decimal amount, string currency, Guid orderId);
}

public class PaymentGatewayResult
{
    public bool Success { get; set; }
    public string? TransactionId { get; set; }
    public string? ErrorMessage { get; set; }
}
