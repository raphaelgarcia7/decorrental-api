using DecorRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DecorRental.Infrastructure.Persistence;

public class DecorRentalDbContext : DbContext
{
    public DbSet<Kit> Kits => Set<Kit>();
    public DbSet<Reservation> Reservations => Set<Reservation>();

    public DecorRentalDbContext(DbContextOptions<DecorRentalDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Kit>()
            .HasMany(kit => kit.Reservations)
            .WithOne()
            .HasForeignKey("KitId");

        base.OnModelCreating(modelBuilder);
    }
}
