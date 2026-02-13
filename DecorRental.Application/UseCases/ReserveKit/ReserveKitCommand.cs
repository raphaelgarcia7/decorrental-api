namespace DecorRental.Application.UseCases.ReserveKit;

public record ReserveKitCommand(
    Guid KitId,
    DateOnly StartDate,
    DateOnly EndDate
);
