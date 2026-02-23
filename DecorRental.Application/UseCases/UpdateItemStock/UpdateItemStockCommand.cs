namespace DecorRental.Application.UseCases.UpdateItemStock;

public sealed record UpdateItemStockCommand(Guid ItemTypeId, int TotalStock);
