using DecorRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace DecorRental.Infrastructure.Persistence;

public class DecorRentalDbContext : DbContext
{
    public DbSet<KitTheme> KitThemes => Set<KitTheme>();
    public DbSet<KitCategory> KitCategories => Set<KitCategory>();
    public DbSet<ItemType> ItemTypes => Set<ItemType>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<ReservationItem> ReservationItems => Set<ReservationItem>();

    public DecorRentalDbContext(DbContextOptions<DecorRentalDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DecorRentalDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
