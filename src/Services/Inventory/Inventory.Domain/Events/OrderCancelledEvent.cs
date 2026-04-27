using MediatR;

namespace Inventory.Domain.Events;

public class OrderCancelledEvent : INotification
{
    public Guid EventId { get; set; }
    public Guid OrderId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
}
