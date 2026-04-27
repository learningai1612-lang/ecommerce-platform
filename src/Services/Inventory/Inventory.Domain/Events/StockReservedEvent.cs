namespace Inventory.Domain.Events;

public class StockReservedEvent
{
    public Guid EventId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public int RemainingAvailable { get; set; }
    public DateTime ReservedAt { get; set; }
}
