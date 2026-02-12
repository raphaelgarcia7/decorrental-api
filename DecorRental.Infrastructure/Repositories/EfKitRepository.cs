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

    public Kit? GetById(Guid id)
        => _context.Kits
            .Include(kit => kit.Reservations)
            .FirstOrDefault(kit => kit.Id == id);

    public IReadOnlyList<Kit> GetAll()
        => _context.Kits
            .Include(kit => kit.Reservations)
            .AsNoTracking()
            .ToList();

    public void Add(Kit kit)
    {
        _context.Kits.Add(kit);
        _context.SaveChanges();
    }

    public void Save(Kit kit)
    {
        if (_context.Entry(kit).State == EntityState.Detached)
        {
            _context.Kits.Attach(kit);
        }

        foreach (var reservation in kit.Reservations)
        {
            var entry = _context.Entry(reservation);
            var exists = _context.Reservations.Any(r => r.Id == reservation.Id);

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

        _context.SaveChanges();
    }
}
