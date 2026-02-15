namespace DecorRental.Application.UseCases.ReserveKit;

public sealed record ReserveKitResult(
    Guid ReservationId,
    Guid KitId,
    DateOnly StartDate,
    DateOnly EndDate,
    string ReservationStatus);
