using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Tests.Application.Fakes;

public class FakeKitRepository : IKitRepository
{
    private readonly Dictionary<Guid, Kit> _storage = new();

    public Task<Kit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_storage.TryGetValue(id, out var kit) ? kit : null);

    public Task<IReadOnlyList<Kit>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = _storage.Values
            .OrderBy(kit => kit.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<Kit>>(items);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_storage.Count);

    public Task AddAsync(Kit kit, CancellationToken cancellationToken = default)
    {
        _storage[kit.Id] = kit;
        return Task.CompletedTask;
    }

    public Task SaveAsync(Kit kit, CancellationToken cancellationToken = default)
    {
        _storage[kit.Id] = kit;
        return Task.CompletedTask;
    }
}
