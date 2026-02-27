namespace DecorRental.Application.UseCases.ReserveKit;

public sealed record ReserveKitCommand(
    Guid KitThemeId,
    Guid KitCategoryId,
    DateOnly StartDate,
    DateOnly EndDate,
    bool AllowStockOverride,
    string? StockOverrideReason,
    string CustomerName,
    string CustomerDocumentNumber,
    string CustomerPhoneNumber,
    string CustomerAddress,
    string? Notes,
    bool HasBalloonArch,
    bool IsEntryPaid);
