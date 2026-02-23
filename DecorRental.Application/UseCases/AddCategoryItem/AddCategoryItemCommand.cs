namespace DecorRental.Application.UseCases.AddCategoryItem;

public sealed record AddCategoryItemCommand(Guid CategoryId, Guid ItemTypeId, int Quantity);
