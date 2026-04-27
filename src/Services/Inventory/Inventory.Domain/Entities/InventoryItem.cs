namespace Inventory.Domain.Entities;

public class InventoryItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }
    public int AvailableQuantity { get; private set; }
    public int ReservedQuantity { get; private set; }
    public int ReorderLevel { get; private set; }
    public byte[] RowVersion { get; private set; } = null!; // Optimistic concurrency
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // EF Core constructor
    private InventoryItem() { }

    public InventoryItem(Guid productId, int initialQuantity, int reorderLevel)
    {
        if (initialQuantity < 0)
            throw new ArgumentException("Initial quantity cannot be negative", nameof(initialQuantity));

        if (reorderLevel < 0)
            throw new ArgumentException("Reorder level cannot be negative", nameof(reorderLevel));

        Id = Guid.NewGuid();
        ProductId = productId;
        AvailableQuantity = initialQuantity;
        ReservedQuantity = 0;
        ReorderLevel = reorderLevel;
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public int TotalQuantity => AvailableQuantity + ReservedQuantity;

    public bool IsLowStock => TotalQuantity <= ReorderLevel;

    public void AddStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to add must be positive", nameof(quantity));

        AvailableQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReserveStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to reserve must be positive", nameof(quantity));

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException(
                $"Cannot reserve {quantity} units. Only {AvailableQuantity} available.");

        AvailableQuantity -= quantity;
        ReservedQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ReleaseReservedStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to release must be positive", nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException(
                $"Cannot release {quantity} units. Only {ReservedQuantity} reserved.");

        ReservedQuantity -= quantity;
        AvailableQuantity += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CommitReservedStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity to commit must be positive", nameof(quantity));

        if (quantity > ReservedQuantity)
            throw new InvalidOperationException(
                $"Cannot commit {quantity} units. Only {ReservedQuantity} reserved.");

        ReservedQuantity -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateReorderLevel(int newReorderLevel)
    {
        if (newReorderLevel < 0)
            throw new ArgumentException("Reorder level cannot be negative", nameof(newReorderLevel));

        ReorderLevel = newReorderLevel;
        UpdatedAt = DateTime.UtcNow;
    }
}
