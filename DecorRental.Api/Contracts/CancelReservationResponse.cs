namespace DecorRental.Api.Contracts;

public sealed record CancelReservationResponse(
    Guid ReservationId,
    Guid KitThemeId,
    string Status,
    string Message);
