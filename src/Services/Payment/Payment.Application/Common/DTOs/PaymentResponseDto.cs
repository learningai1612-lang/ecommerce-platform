using Payment.Domain.Enums;

namespace Payment.Application.Common.DTOs;

public class PaymentResponseDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    public PaymentStatus Status { get; set; }
    public string? TransactionId { get; set; }
    public string? FailureReason { get; set; }
    public DateTime? ProcessedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}
