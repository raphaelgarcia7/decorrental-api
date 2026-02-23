namespace DecorRental.Api.Contracts;

public sealed record CreateCategoryRequest(string Name);

public sealed record AddCategoryItemRequest(Guid ItemTypeId, int Quantity);

public sealed record CategoryItemResponse(Guid ItemTypeId, int Quantity);

public sealed record CategoryResponse(Guid Id, string Name, IReadOnlyList<CategoryItemResponse> Items);
