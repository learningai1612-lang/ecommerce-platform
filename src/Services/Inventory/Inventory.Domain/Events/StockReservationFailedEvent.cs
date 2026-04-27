namespace Inventory.Domain.Events;

public class StockReservationFailedEvent
{
    public Guid EventId { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductId { get; set; }
    public int RequestedQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}
