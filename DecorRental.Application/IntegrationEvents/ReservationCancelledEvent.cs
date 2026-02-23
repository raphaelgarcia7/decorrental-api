using DecorRental.Application.Messaging;

namespace DecorRental.Application.IntegrationEvents;

public sealed record ReservationCancelledEvent(
    Guid KitThemeId,
    Guid KitCategoryId,
    Guid ReservationId,
    string Status) : IIntegrationEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTimeOffset OccurredAtUtc { get; } = DateTimeOffset.UtcNow;
    public string RoutingKey => "reservation.cancelled";
}
