namespace Order.Domain.Events;

public class OrderPaidEvent
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime PaidAt { get; set; }
}
