using DecorRental.Domain.Entities;

namespace DecorRental.Application.UseCases.GetItemTypes;

public sealed record GetItemTypesResult(IReadOnlyList<ItemType> Items);
