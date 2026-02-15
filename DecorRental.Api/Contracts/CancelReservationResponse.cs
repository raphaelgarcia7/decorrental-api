namespace DecorRental.Api.Contracts;

public sealed record CancelReservationResponse(
    Guid ReservationId,
    Guid KitId,
    string Status,
    string Message);
