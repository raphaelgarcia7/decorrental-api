namespace DecorRental.Api.Contracts;

public sealed record KitSummaryResponse(Guid Id, string Name);

public sealed record ReservationResponse(
    Guid Id,
    Guid KitCategoryId,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status);

public sealed record KitDetailResponse(
    Guid Id,
    string Name,
    IReadOnlyList<ReservationResponse> Reservations);
