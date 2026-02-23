namespace DecorRental.Api.Contracts;

public sealed record ReserveKitRequest(Guid KitCategoryId, DateOnly StartDate, DateOnly EndDate);
