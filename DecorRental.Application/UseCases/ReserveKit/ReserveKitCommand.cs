namespace DecorRental.Application.UseCases.ReserveKit;

public sealed record ReserveKitCommand(
    Guid KitThemeId,
    Guid KitCategoryId,
    DateOnly StartDate,
    DateOnly EndDate);
