using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Tests.Application.Fakes;

public class FakeKitRepository : IKitRepository
{
    private readonly Dictionary<Guid, Kit> _storage = new();

    public Kit? GetById(Guid id)
        => _storage.TryGetValue(id, out var kit) ? kit : null;

    public void Save(Kit kit)
        => _storage[kit.Id] = kit;

    public void Add(Kit kit)
        => _storage[kit.Id] = kit;
}
