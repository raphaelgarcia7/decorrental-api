using DecorRental.Domain.Entities;

namespace DecorRental.Domain.Repositories;

public interface IItemTypeRepository
{
    Task<ItemType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ItemType>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ItemType>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(ItemType itemType, CancellationToken cancellationToken = default);
    Task SaveAsync(ItemType itemType, CancellationToken cancellationToken = default);
}
