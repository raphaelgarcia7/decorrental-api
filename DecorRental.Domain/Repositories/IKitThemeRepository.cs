using DecorRental.Domain.Entities;

namespace DecorRental.Domain.Repositories;

public interface IKitThemeRepository
{
    Task<KitTheme?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<KitTheme>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<int> CountAsync(CancellationToken cancellationToken = default);
    Task AddAsync(KitTheme kitTheme, CancellationToken cancellationToken = default);
    Task SaveAsync(KitTheme kitTheme, CancellationToken cancellationToken = default);
}
