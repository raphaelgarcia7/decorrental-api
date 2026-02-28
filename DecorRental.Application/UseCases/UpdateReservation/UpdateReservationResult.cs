namespace DecorRental.Application.UseCases.UpdateReservation;

public sealed record UpdateReservationResult(
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
    string CustomerPhoneNumber,
    string CustomerAddress,
    string? Notes,
    bool HasBalloonArch,
    bool IsEntryPaid);
