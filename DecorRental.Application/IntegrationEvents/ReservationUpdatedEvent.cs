using DecorRental.Application.Messaging;

namespace DecorRental.Application.IntegrationEvents;

public sealed record ReservationUpdatedEvent(
    Guid KitThemeId,
    Guid KitCategoryId,
    Guid ReservationId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
    public string RoutingKey => "reservation.updated";
}
