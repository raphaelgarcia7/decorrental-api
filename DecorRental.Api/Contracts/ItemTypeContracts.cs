namespace DecorRental.Api.Contracts;

public sealed record CreateItemTypeRequest(string Name, int TotalStock);

public sealed record UpdateItemStockRequest(int TotalStock);

public sealed record ItemTypeResponse(Guid Id, string Name, int TotalStock);
