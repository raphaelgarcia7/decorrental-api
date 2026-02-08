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

    public void Save(Kit kit)
    {
        _context.Update(kit);
        _context.SaveChanges();
    }
}
