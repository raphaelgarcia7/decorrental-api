namespace DecorRental.Application.Contracts;

public sealed record ContractData(
    Guid KitThemeId,
    Guid ReservationId,
    string KitThemeName,
    string KitCategoryName,
    DateOnly ReservationStartDate,
    DateOnly ReservationEndDate,
    string CustomerName,
    string CustomerDocumentNumber,
    string CustomerPhoneNumber,
    string CustomerAddress,
    string? CustomerNeighborhood,
    string? CustomerCity,
    string? Notes,
    bool HasBalloonArch,
    bool IsEntryPaid,
    DateOnly ContractDate,
    decimal? TotalAmount = null,
    decimal? EntryAmount = null);
