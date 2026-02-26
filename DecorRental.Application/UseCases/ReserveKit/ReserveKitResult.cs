namespace DecorRental.Application.UseCases.ReserveKit;

public sealed record ReserveKitResult(
    Guid ReservationId,
    Guid KitThemeId,
    Guid KitCategoryId,
    DateOnly StartDate,
    DateOnly EndDate,
    string ReservationStatus,
    bool IsStockOverride,
    string? StockOverrideReason,
    string CustomerName,
    string CustomerDocumentNumber,
    string CustomerAddress,
    string? Notes,
    bool HasBalloonArch);
