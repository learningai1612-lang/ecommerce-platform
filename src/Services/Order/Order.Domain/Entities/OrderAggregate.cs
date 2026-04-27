using Order.Domain.Common;
using Order.Domain.Enums;
using Order.Domain.Events;

namespace Order.Domain.Entities;

/// <summary>
/// Order Aggregate Root
/// Encapsulates business rules and maintains consistency
/// </summary>
public class OrderAggregate : BaseEntity
{
    private readonly List<OrderItem> _items = new();
    private readonly List<object> _domainEvents = new();

    public Guid UserId { get; private set; }
    public string ShippingAddress { get; private set; }
    public OrderStatus Status { get; private set; }
    public decimal TotalAmount { get; private set; }
    public string? Notes { get; private set; }

    // Navigation properties
    public virtual IReadOnlyCollection<OrderItem> Items => _items.AsReadOnly();

    // Domain events
    public IReadOnlyCollection<object> DomainEvents => _domainEvents.AsReadOnly();

    // Required by EF Core
    private OrderAggregate()
    {
        ShippingAddress = string.Empty;
    }

    public OrderAggregate(Guid userId, string shippingAddress, List<OrderItem> items, string? notes = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("UserId is required.", nameof(userId));

        if (string.IsNullOrWhiteSpace(shippingAddress))
            throw new ArgumentException("Shipping address is required.", nameof(shippingAddress));

        if (items == null || !items.Any())
            throw new ArgumentException("Order must have at least one item.", nameof(items));

        UserId = userId;
        ShippingAddress = shippingAddress;
        Status = OrderStatus.Pending;
        Notes = notes;

        _items.AddRange(items);
        CalculateTotalAmount();

        // Raise domain event
        AddDomainEvent(new OrderCreatedEvent
        {
            OrderId = Id,
            UserId = UserId,
            TotalAmount = TotalAmount,
            ItemCount = _items.Count,
            CreatedAt = CreatedAt
        });
    }

    /// <summary>
    /// Mark order as payment pending
    /// </summary>
    public void MarkAsPaymentPending()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot mark order as payment pending from status: {Status}");

        Status = OrderStatus.PaymentPending;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark order as paid - successful payment
    /// </summary>
    public void MarkAsPaid()
    {
        if (Status != OrderStatus.PaymentPending && Status != OrderStatus.Pending)
            throw new InvalidOperationException($"Cannot mark order as paid from status: {Status}");

        Status = OrderStatus.Paid;
        UpdatedAt = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new OrderPaidEvent
        {
            OrderId = Id,
            UserId = UserId,
            TotalAmount = TotalAmount,
            PaidAt = UpdatedAt
        });
    }

    /// <summary>
    /// Mark order as failed - payment failed
    /// </summary>
    public void MarkAsFailed(string reason)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot mark completed/shipped order as failed");

        Status = OrderStatus.Failed;
        Notes = $"{Notes} | Failed: {reason}";
        UpdatedAt = DateTime.UtcNow;

        // Raise domain event
        AddDomainEvent(new OrderFailedEvent
        {
            OrderId = Id,
            UserId = UserId,
            Reason = reason,
            FailedAt = UpdatedAt
        });
    }

    /// <summary>
    /// Cancel order - user-initiated or system
    /// </summary>
    public void Cancel(string reason)
    {
        if (Status == OrderStatus.Completed || Status == OrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot cancel completed/shipped order");

        Status = OrderStatus.Cancelled;
        Notes = $"{Notes} | Cancelled: {reason}";
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark order as shipped - can only ship paid orders
    /// </summary>
    public void MarkAsShipped()
    {
        if (Status != OrderStatus.Paid)
            throw new InvalidOperationException($"Cannot ship unpaid order. Current status: {Status}");

        Status = OrderStatus.Shipped;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark order as completed - final state
    /// </summary>
    public void MarkAsCompleted()
    {
        if (Status != OrderStatus.Shipped)
            throw new InvalidOperationException($"Cannot complete order that hasn't been shipped. Current status: {Status}");

        Status = OrderStatus.Completed;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Update shipping address (only if not shipped yet)
    /// </summary>
    public void UpdateShippingAddress(string newAddress)
    {
        if (Status == OrderStatus.Shipped || Status == OrderStatus.Completed)
            throw new InvalidOperationException("Cannot update shipping address for shipped/completed orders");

        if (string.IsNullOrWhiteSpace(newAddress))
            throw new ArgumentException("Shipping address cannot be empty", nameof(newAddress));

        ShippingAddress = newAddress;
        UpdatedAt = DateTime.UtcNow;
    }

    private void CalculateTotalAmount()
    {
        TotalAmount = _items.Sum(item => item.Subtotal);
    }

    private void AddDomainEvent(object domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
