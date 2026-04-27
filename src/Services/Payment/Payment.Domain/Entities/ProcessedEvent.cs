namespace Payment.Domain.Entities;

/// <summary>
/// ProcessedEvent - Tracks processed events for idempotency
/// </summary>
public class ProcessedEvent
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EventId { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public DateTime ProcessedAt { get; set; }
    public string Payload { get; set; } = string.Empty;
}
