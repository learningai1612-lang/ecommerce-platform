using Microsoft.Extensions.Logging;
using Payment.Application.Common.Interfaces;

namespace Payment.Infrastructure.Services;

public class InMemoryEventPublisher : IEventPublisher
{
    private readonly ILogger<InMemoryEventPublisher> _logger;

    public InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger)
    {
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken cancellationToken = default) where TEvent : class
    {
        // TODO: Integrate with message broker (RabbitMQ, Azure Service Bus, etc.)
        // For now, just log the event
        _logger.LogInformation(
            "Publishing event {EventType}: {@Event}",
            typeof(TEvent).Name,
            @event);

        return Task.CompletedTask;
    }
}
