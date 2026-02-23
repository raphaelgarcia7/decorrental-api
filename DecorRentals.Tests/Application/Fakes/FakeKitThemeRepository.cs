using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Tests.Application.Fakes;

public sealed class FakeKitThemeRepository : IKitThemeRepository
{
    private readonly Dictionary<Guid, KitTheme> _storage = new();

    public Task<KitTheme?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => Task.FromResult(_storage.TryGetValue(id, out var kitTheme) ? kitTheme : null);

    public Task<IReadOnlyList<KitTheme>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var items = _storage.Values
            .OrderBy(kitTheme => kitTheme.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return Task.FromResult<IReadOnlyList<KitTheme>>(items);
    }

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_storage.Count);

    public Task AddAsync(KitTheme kitTheme, CancellationToken cancellationToken = default)
    {
        _storage[kitTheme.Id] = kitTheme;
        return Task.CompletedTask;
    }

    public Task SaveAsync(KitTheme kitTheme, CancellationToken cancellationToken = default)
    {
        _storage[kitTheme.Id] = kitTheme;
        return Task.CompletedTask;
    }
}
