namespace DecorRental.Api.Contracts;

public sealed record ContractDataRequest(
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
    string? Notes,
    bool HasBalloonArch,
    bool IsEntryPaid,
    DateOnly ContractDate);

public sealed record ContractDataResponse(
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
    string? Notes,
    bool HasBalloonArch,
    bool IsEntryPaid,
    DateOnly ContractDate);
