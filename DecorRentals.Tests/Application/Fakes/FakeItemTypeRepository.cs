using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Tests.Application.Fakes;

public sealed class FakeItemTypeRepository : IItemTypeRepository
{
    private readonly Dictionary<Guid, ItemType> _storage = new();

    public Task<ItemType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_storage.TryGetValue(id, out var itemType) ? itemType : null);

    public Task<IReadOnlyList<ItemType>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        var items = _storage.Values
            .Where(itemType => ids.Contains(itemType.Id))
            .ToList();

        return Task.FromResult<IReadOnlyList<ItemType>>(items);
    }

    public Task<IReadOnlyList<ItemType>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<ItemType>>(_storage.Values.OrderBy(itemType => itemType.Name).ToList());

    public Task AddAsync(ItemType itemType, CancellationToken cancellationToken = default)
    {
        _storage[itemType.Id] = itemType;
        return Task.CompletedTask;
    }

    public Task SaveAsync(ItemType itemType, CancellationToken cancellationToken = default)
    {
        _storage[itemType.Id] = itemType;
        return Task.CompletedTask;
    }
}
