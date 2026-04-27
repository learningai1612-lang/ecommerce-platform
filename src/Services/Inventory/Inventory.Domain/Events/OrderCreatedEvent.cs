using MediatR;

namespace Inventory.Domain.Events;

public class OrderCreatedEvent : INotification
{
    public Guid EventId { get; set; }
    public Guid OrderId { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class OrderItemDto
{
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
}
