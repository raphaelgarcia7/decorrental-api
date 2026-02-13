using DecorRental.Domain.Entities;

namespace DecorRental.Application.UseCases.GetKits;

public sealed record GetKitsResult(
    IReadOnlyList<Kit> Items,
    int Page,
    int PageSize,
    int TotalCount
);
