namespace DecorRental.Api.Contracts;

public sealed record ReserveKitRequest(
    Guid KitCategoryId,
    DateOnly StartDate,
    DateOnly EndDate,
    bool AllowStockOverride = false,
    string? StockOverrideReason = null,
    string CustomerName = "",
    string CustomerDocumentNumber = "",
    string CustomerAddress = "",
    string? Notes = null,
    bool HasBalloonArch = false);
