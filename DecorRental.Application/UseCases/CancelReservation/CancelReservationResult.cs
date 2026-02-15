namespace DecorRental.Application.UseCases.CancelReservation;

public sealed record CancelReservationResult(
    Guid ReservationId,
    Guid KitId,
    string ReservationStatus);
