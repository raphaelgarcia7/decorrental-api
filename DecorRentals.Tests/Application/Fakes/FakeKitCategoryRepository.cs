using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Tests.Application.Fakes;

public sealed class FakeKitCategoryRepository : IKitCategoryRepository
{
    private readonly Dictionary<Guid, KitCategory> _storage = new();

    public Task<KitCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_storage.TryGetValue(id, out var category) ? category : null);

    public Task<IReadOnlyList<KitCategory>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult<IReadOnlyList<KitCategory>>(_storage.Values.OrderBy(category => category.Name).ToList());

    public Task AddAsync(KitCategory category, CancellationToken cancellationToken = default)
    {
        _storage[category.Id] = category;
        return Task.CompletedTask;
    }

    public Task SaveAsync(KitCategory category, CancellationToken cancellationToken = default)
    {
        _storage[category.Id] = category;
        return Task.CompletedTask;
    }
}
