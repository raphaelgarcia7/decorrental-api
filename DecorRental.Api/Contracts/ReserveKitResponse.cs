namespace DecorRental.Api.Contracts;

public sealed record ReserveKitResponse(
    Guid ReservationId,
    Guid KitId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    string Message);
