using Cart.Domain.Common;

namespace Cart.Domain.Entities;

/// <summary>
/// ShoppingCart aggregate root - Manages cart items for a user
/// </summary>
public class ShoppingCart : BaseEntity
{
    private readonly List<CartItem> _items = new();

    public Guid UserId { get; private set; }
    public IReadOnlyCollection<CartItem> Items => _items.AsReadOnly();
    public decimal TotalAmount => _items.Sum(item => item.Subtotal);
    public int TotalItems => _items.Sum(item => item.Quantity);

    // Required for serialization
    private ShoppingCart()
    {
    }

    public ShoppingCart(Guid userId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID is required.", nameof(userId));

        UserId = userId;
    }

    /// <summary>
    /// Add item to cart or increase quantity if already exists
    /// </summary>
    public void AddItem(Guid productId, string productName, decimal price, int quantity)
    {
        var existingItem = _items.FirstOrDefault(i => i.ProductId == productId);

        if (existingItem != null)
        {
            // Product already in cart - increase quantity instead of adding duplicate
            existingItem.IncreaseQuantity(quantity);
        }
        else
        {
            // New product - add to cart
            var newItem = new CartItem(productId, productName, price, quantity);
            _items.Add(newItem);
        }

        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update quantity of existing cart item
    /// </summary>
    public void UpdateItemQuantity(Guid itemId, int newQuantity)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        
        if (item == null)
            throw new InvalidOperationException($"Cart item with ID {itemId} not found.");

        item.UpdateQuantity(newQuantity);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Remove item from cart
    /// </summary>
    public void RemoveItem(Guid itemId)
    {
        var item = _items.FirstOrDefault(i => i.Id == itemId);
        
        if (item == null)
            throw new InvalidOperationException($"Cart item with ID {itemId} not found.");

        _items.Remove(item);
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Clear all items from cart
    /// </summary>
    public void Clear()
    {
        _items.Clear();
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Check if cart is empty
    /// </summary>
    public bool IsEmpty() => _items.Count == 0;
}
