using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;
using DecorRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DecorRental.Infrastructure.Repositories;

public sealed class EfItemTypeRepository : IItemTypeRepository
{
    private readonly DecorRentalDbContext _context;

    public EfItemTypeRepository(DecorRentalDbContext context)
    {
        _context = context;
    }

    public Task<ItemType?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.ItemTypes.FirstOrDefaultAsync(itemType => itemType.Id == id, cancellationToken);

    public async Task<IReadOnlyList<ItemType>> GetByIdsAsync(IReadOnlyCollection<Guid> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        return await _context.ItemTypes
            .Where(itemType => ids.Contains(itemType.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ItemType>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.ItemTypes
            .AsNoTracking()
            .OrderBy(itemType => itemType.Name)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(ItemType itemType, CancellationToken cancellationToken = default)
    {
        await _context.ItemTypes.AddAsync(itemType, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(ItemType itemType, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(itemType).State == EntityState.Detached)
        {
            throw new InvalidOperationException("Item type must be tracked before saving changes.");
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
