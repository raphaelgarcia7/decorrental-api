using DecorRental.Domain.Entities;

namespace DecorRental.Domain.Repositories;

public interface IKitRepository
{
    Task<Kit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Kit>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(Kit kit, CancellationToken cancellationToken = default);
    Task SaveAsync(Kit kit, CancellationToken cancellationToken = default);
}
