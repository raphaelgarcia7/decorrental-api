using DecorRental.Domain.Entities;

namespace DecorRental.Application.UseCases.GetKitCategories;

public sealed record GetKitCategoriesResult(IReadOnlyList<KitCategory> Items);
