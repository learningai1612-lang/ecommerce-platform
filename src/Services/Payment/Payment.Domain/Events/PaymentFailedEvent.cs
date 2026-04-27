namespace Payment.Domain.Events;

public class PaymentFailedEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}
