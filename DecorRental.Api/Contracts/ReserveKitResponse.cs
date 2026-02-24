namespace DecorRental.Api.Contracts;

public sealed record ReserveKitResponse(
    Guid ReservationId,
    Guid KitThemeId,
    Guid KitCategoryId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status,
    bool IsStockOverride,
    string? StockOverrideReason,
    string Message);
