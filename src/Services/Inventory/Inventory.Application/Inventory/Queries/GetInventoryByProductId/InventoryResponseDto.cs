namespace Inventory.Application.Inventory.Queries.GetInventoryByProductId;

public class InventoryResponseDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    public int TotalQuantity { get; set; }
    public int ReorderLevel { get; set; }
    public bool IsLowStock { get; set; }
    public DateTime UpdatedAt { get; set; }
}
