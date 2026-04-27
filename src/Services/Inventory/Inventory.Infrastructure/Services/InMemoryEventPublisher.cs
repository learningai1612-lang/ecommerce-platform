using Inventory.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;

namespace Inventory.Infrastructure.Services;

public class InMemoryEventPublisher : IEventPublisher
{
    private readonly ILogger<InMemoryEventPublisher> _logger;

    public InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken = default) where T : class
    {
        var eventType = @event.GetType().Name;
        
        _logger.LogInformation(
            "Publishing event {EventType}: {Event}",
            eventType,
            System.Text.Json.JsonSerializer.Serialize(@event));

        // In production, this would publish to a message broker like:
        // - RabbitMQ
        // - Azure Service Bus
        // - Apache Kafka
        // - AWS SNS/SQS

        return Task.CompletedTask;
    }
}
