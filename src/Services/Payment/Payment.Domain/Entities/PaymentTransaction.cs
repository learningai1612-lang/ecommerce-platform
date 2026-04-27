using Payment.Domain.Common;
using Payment.Domain.Enums;

namespace Payment.Domain.Entities;

/// <summary>
/// Payment entity - Represents a payment transaction
/// </summary>
public class PaymentTransaction : BaseEntity
{
    public Guid OrderId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    public PaymentStatus Status { get; private set; }
    public string? TransactionId { get; private set; }
    public string? FailureReason { get; private set; }
    public DateTime? ProcessedAt { get; private set; }

    // Required by EF Core
    private PaymentTransaction()
    {
        Currency = "USD";
    }

    public PaymentTransaction(Guid orderId, decimal amount, string currency = "USD")
    {
        if (orderId == Guid.Empty)
            throw new ArgumentException("Order ID is required.", nameof(orderId));

        if (amount <= 0)
            throw new ArgumentException("Amount must be greater than zero.", nameof(amount));

        if (string.IsNullOrWhiteSpace(currency))
            throw new ArgumentException("Currency is required.", nameof(currency));

        OrderId = orderId;
        Amount = amount;
        Currency = currency;
        Status = PaymentStatus.Pending;
    }

    /// <summary>
    /// Mark payment as processing
    /// </summary>
    public void MarkAsProcessing()
    {
        if (Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot process payment in status: {Status}");

        Status = PaymentStatus.Processing;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark payment as completed
    /// </summary>
    public void MarkAsCompleted(string transactionId)
    {
        if (Status != PaymentStatus.Processing && Status != PaymentStatus.Pending)
            throw new InvalidOperationException($"Cannot complete payment in status: {Status}");

        if (string.IsNullOrWhiteSpace(transactionId))
            throw new ArgumentException("Transaction ID is required.", nameof(transactionId));

        Status = PaymentStatus.Completed;
        TransactionId = transactionId;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark payment as failed
    /// </summary>
    public void MarkAsFailed(string reason)
    {
        if (Status == PaymentStatus.Completed)
            throw new InvalidOperationException("Cannot fail a completed payment");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Failure reason is required.", nameof(reason));

        Status = PaymentStatus.Failed;
        FailureReason = reason;
        ProcessedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
