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

        modelBuilder.Entity<Reservation>()
            .OwnsOne(r => r.Period, period =>
            {
                period.Property(p => p.Start)
                    .HasColumnName("StartDate")
                    .HasConversion(
                        d => d.ToDateTime(TimeOnly.MinValue),
                        d => DateOnly.FromDateTime(d))
                    .IsRequired();

                period.Property(p => p.End)
                    .HasColumnName("EndDate")
                    .HasConversion(
                        d => d.ToDateTime(TimeOnly.MinValue),
                        d => DateOnly.FromDateTime(d))
                    .IsRequired();
            });

        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DecorRentalDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
