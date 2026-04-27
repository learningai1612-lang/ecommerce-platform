namespace Inventory.Domain.Events;

public class StockReleasedEvent
{
    public Guid EventId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime ReleasedAt { get; set; }
}
