using MediatR;

namespace Inventory.Domain.Events;

public class PaymentSucceededEvent : INotification
{
    public Guid EventId { get; set; }
    public Guid OrderId { get; set; }
    public Guid PaymentId { get; set; }
    public DateTime ProcessedAt { get; set; }
}
