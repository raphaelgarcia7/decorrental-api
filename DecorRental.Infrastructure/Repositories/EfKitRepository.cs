using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;
using DecorRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DecorRental.Infrastructure.Repositories;

public class EfKitRepository : IKitRepository
{
    private readonly DecorRentalDbContext _context;

    public EfKitRepository(DecorRentalDbContext context)
    {
        _context = context;
    }

    public Task<Kit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.Kits
            .Include(kit => kit.Reservations)
            .FirstOrDefaultAsync(kit => kit.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Kit>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => await _context.Kits
            .Include(kit => kit.Reservations)
            .AsNoTracking()
            .OrderBy(kit => kit.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _context.Kits.CountAsync(cancellationToken);

    public async Task AddAsync(Kit kit, CancellationToken cancellationToken = default)
    {
        await _context.Kits.AddAsync(kit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(Kit kit, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(kit).State == EntityState.Detached)
        {
            throw new InvalidOperationException("Kit must be tracked before saving changes.");
        }

        var existingReservationIds = await _context.Reservations
            .AsNoTracking()
            .Where(reservation => reservation.KitId == kit.Id)
            .Select(reservation => reservation.Id)
            .ToHashSetAsync(cancellationToken);

        foreach (var reservation in kit.Reservations)
        {
            if (!existingReservationIds.Contains(reservation.Id))
            {
                _context.Entry(reservation).State = EntityState.Added;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
