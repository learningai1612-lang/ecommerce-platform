using MediatR;

namespace Payment.Domain.Events;

public class OrderCreatedEvent : INotification
{
    public Guid EventId { get; set; }
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
