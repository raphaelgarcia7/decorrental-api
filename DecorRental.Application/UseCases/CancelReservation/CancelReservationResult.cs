namespace DecorRental.Application.UseCases.CancelReservation;

public sealed record CancelReservationResult(
    Guid ReservationId,
    Guid KitThemeId,
    string ReservationStatus);
