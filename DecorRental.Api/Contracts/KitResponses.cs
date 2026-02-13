namespace DecorRental.Api.Contracts;

public record KitSummaryResponse(Guid Id, string Name);

public record ReservationResponse(
    Guid Id,
    DateOnly StartDate,
    DateOnly EndDate,
    string Status
);

public record KitDetailResponse(
    Guid Id,
    string Name,
    IReadOnlyList<ReservationResponse> Reservations
);
