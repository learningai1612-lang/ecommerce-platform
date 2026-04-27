namespace Payment.Domain.Events;

public class PaymentSucceededEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
}
