using DecorRental.Domain.Entities;

namespace DecorRental.Application.UseCases.GetKits;

public sealed record GetKitsResult(
    IReadOnlyList<KitTheme> Items,
    int Page,
    int PageSize,
    int TotalCount);
