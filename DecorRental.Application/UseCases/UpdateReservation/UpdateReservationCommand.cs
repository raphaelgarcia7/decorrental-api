namespace DecorRental.Application.UseCases.UpdateReservation;

public sealed record UpdateReservationCommand(
    Guid KitThemeId,
    Guid ReservationId,
    Guid KitCategoryId,
    DateOnly StartDate,
    DateOnly EndDate,
    bool AllowStockOverride = false,
    string? StockOverrideReason = null,
    string CustomerName = "",
    string CustomerDocumentNumber = "",
    string CustomerPhoneNumber = "",
    string CustomerAddress = "",
    string? Notes = null,
    bool HasBalloonArch = false,
    bool IsEntryPaid = false);
