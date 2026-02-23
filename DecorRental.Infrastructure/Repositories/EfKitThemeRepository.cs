using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;
using DecorRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DecorRental.Infrastructure.Repositories;

public sealed class EfKitThemeRepository : IKitThemeRepository
{
    private readonly DecorRentalDbContext _context;

    public EfKitThemeRepository(DecorRentalDbContext context)
    {
        _context = context;
    }

    public Task<KitTheme?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _context.KitThemes
            .Include(kitTheme => kitTheme.Reservations)
            .ThenInclude(reservation => reservation.Items)
            .FirstOrDefaultAsync(kitTheme => kitTheme.Id == id, cancellationToken);

    public async Task<IReadOnlyList<KitTheme>> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        => await _context.KitThemes
            .Include(kitTheme => kitTheme.Reservations)
            .ThenInclude(reservation => reservation.Items)
            .AsNoTracking()
            .OrderBy(kitTheme => kitTheme.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

    public Task<int> CountAsync(CancellationToken cancellationToken = default)
        => _context.KitThemes.CountAsync(cancellationToken);

    public async Task AddAsync(KitTheme kitTheme, CancellationToken cancellationToken = default)
    {
        await _context.KitThemes.AddAsync(kitTheme, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveAsync(KitTheme kitTheme, CancellationToken cancellationToken = default)
    {
        if (_context.Entry(kitTheme).State == EntityState.Detached)
        {
            throw new InvalidOperationException("Kit theme must be tracked before saving changes.");
        }

        _context.ChangeTracker.DetectChanges();

        foreach (var reservation in kitTheme.Reservations)
        {
            var reservationEntry = _context.Entry(reservation);
            if (reservationEntry.State == EntityState.Detached)
            {
                _context.Attach(reservation);
                reservationEntry = _context.Entry(reservation);
            }

            if (reservationEntry.State is EntityState.Unchanged or EntityState.Modified)
            {
                var reservationExists = await _context.Set<Reservation>()
                    .AsNoTracking()
                    .AnyAsync(existingReservation => existingReservation.Id == reservation.Id, cancellationToken);

                if (!reservationExists)
                {
                    reservationEntry.State = EntityState.Added;
                }
            }

            foreach (var reservationItem in reservation.Items)
            {
                var reservationItemEntry = _context.Entry(reservationItem);
                if (reservationItemEntry.State == EntityState.Detached)
                {
                    _context.Attach(reservationItem);
                    reservationItemEntry = _context.Entry(reservationItem);
                }

                if (reservationItemEntry.State is EntityState.Unchanged or EntityState.Modified)
                {
                    var reservationItemExists = await _context.Set<ReservationItem>()
                        .AsNoTracking()
                        .AnyAsync(existingReservationItem => existingReservationItem.Id == reservationItem.Id, cancellationToken);

                    if (!reservationItemExists)
                    {
                        reservationItemEntry.State = EntityState.Added;
                    }
                }
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
