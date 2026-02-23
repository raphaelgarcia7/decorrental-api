using DecorRental.Domain.Repositories;

namespace DecorRental.Tests.Application.Fakes;

public sealed class FakeReservationQueryRepository : IReservationQueryRepository
{
    private readonly List<ActiveReservationItem> _items = new();

    public Task<IReadOnlyList<ActiveReservationItem>> GetActiveReservationItemsAsync(
        DateOnly requestStartDate,
        DateOnly requestEndDate,
        IReadOnlyCollection<Guid> itemTypeIds,
        CancellationToken cancellationToken = default)
    {
        var items = _items
            .Where(item => itemTypeIds.Contains(item.ItemTypeId))
            .Where(item => item.StartDate <= requestEndDate && item.EndDate >= requestStartDate)
            .ToList();

        return Task.FromResult<IReadOnlyList<ActiveReservationItem>>(items);
    }

    public void Add(ActiveReservationItem activeReservationItem)
    {
        _items.Add(activeReservationItem);
    }
}
