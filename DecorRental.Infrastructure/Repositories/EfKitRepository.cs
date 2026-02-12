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

    public async Task<IReadOnlyList<Kit>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Kits
            .Include(kit => kit.Reservations)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

    public async Task AddAsync(Kit kit, CancellationToken cancellationToken = default)
    {
        await _context.Kits.AddAsync(kit, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(Kit kit, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(kit).State == EntityState.Detached)
        {
            _context.Kits.Attach(kit);
        }

        foreach (var reservation in kit.Reservations)
        {
            var entry = _context.Entry(reservation);
            var exists = await _context.Reservations.AnyAsync(
                r => r.Id == reservation.Id,
                cancellationToken);

            if (!exists)
            {
                entry.State = EntityState.Added;
                continue;
            }

            if (entry.State == EntityState.Detached)
            {
                entry.State = EntityState.Modified;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
