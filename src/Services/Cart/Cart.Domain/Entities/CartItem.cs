using Cart.Domain.Common;

namespace Cart.Domain.Entities;

/// <summary>
/// CartItem entity - Snapshot of product added to cart
/// </summary>
public class CartItem : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string ProductName { get; private set; }
    public decimal Price { get; private set; }
    public int Quantity { get; private set; }
    public decimal Subtotal => Price * Quantity;

    // Required for serialization
    private CartItem()
    {
        ProductName = string.Empty;
    }

    public CartItem(Guid productId, string productName, decimal price, int quantity)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException("Product ID is required.", nameof(productId));

        if (string.IsNullOrWhiteSpace(productName))
            throw new ArgumentException("Product name is required.", nameof(productName));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(price));

        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));

        ProductId = productId;
        ProductName = productName;
        Price = price;
        Quantity = quantity;
    }

    /// <summary>
    /// Update quantity - ensures quantity stays positive
    /// </summary>
    public void UpdateQuantity(int newQuantity)
    {
        if (newQuantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(newQuantity));

        Quantity = newQuantity;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Increase quantity by specified amount
    /// </summary>
    public void IncreaseQuantity(int amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        Quantity += amount;
        UpdatedAt = DateTime.UtcNow;
    }
}
