namespace Order.Domain.Events;

public class OrderFailedEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}
