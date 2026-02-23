using DecorRental.Domain.Entities;

namespace DecorRental.Domain.Repositories;

public interface IKitCategoryRepository
{
    Task<KitCategory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KitCategory>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(KitCategory category, CancellationToken cancellationToken = default);
    Task SaveAsync(KitCategory category, CancellationToken cancellationToken = default);
}
