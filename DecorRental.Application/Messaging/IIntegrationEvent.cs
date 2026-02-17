namespace DecorRental.Application.Messaging;

public interface IIntegrationEvent
{
    Guid EventId { get; }
    DateTimeOffset OccurredAtUtc { get; }
    string RoutingKey { get; }
}
