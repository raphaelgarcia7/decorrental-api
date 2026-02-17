using DecorRental.Application.Messaging;

namespace DecorRental.Tests.Application.Fakes;

public sealed class FakeMessageBus : IMessageBus
{
    public List<IIntegrationEvent> PublishedEvents { get; } = new();

    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        PublishedEvents.Add(integrationEvent);
        return Task.CompletedTask;
    }
}
