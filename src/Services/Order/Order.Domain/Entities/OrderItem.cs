using Order.Domain.Common;

namespace Order.Domain.Entities;

/// <summary>
/// OrderItem entity - Snapshot of product at purchase time
/// Part of Order aggregate
/// </summary>
public class OrderItem : BaseEntity
{
    public Guid OrderId { get; private set; }
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal UnitPrice { get; private set; }
    public int Quantity { get; private set; }
    public decimal Subtotal { get; private set; }

    // Navigation property
    public virtual OrderAggregate Order { get; private set; } = null!;

    // Required by EF Core
    private OrderItem() 
    {
        ProductName = string.Empty;
    }

    public OrderItem(Guid productId, string productName, decimal unitPrice, int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));

        ProductId = productId;
        ProductName = productName;
        UnitPrice = unitPrice;
        Quantity = quantity;
        Subtotal = unitPrice * quantity;
    }

    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));

        Quantity = newQuantity;
        Subtotal = UnitPrice * Quantity;
        UpdatedAt = DateTime.UtcNow;
    }
}
