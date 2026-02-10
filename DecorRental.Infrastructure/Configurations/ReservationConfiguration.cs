using DecorRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DecorRental.Infrastructure.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.HasKey(reservation => reservation.Id);

        // Armazena enum como int no banco
        builder.Property(reservation => reservation.Status).HasConversion<int>();

        // DateRange é Value Object, não vira tabela própria
        builder.OwnsOne(reservation => reservation.Period, period =>
        {
            // SQLite não lida bem com DateOnly, então convertemos para DateTime
            period.Property(p => p.Start)
                .HasColumnName("StartDate")
                .HasConversion(
                    v => v.ToDateTime(TimeOnly.MinValue),
                    v => DateOnly.FromDateTime(v));

            period.Property(p => p.End)
                .HasColumnName("EndDate")
                .HasConversion(
                    v => v.ToDateTime(TimeOnly.MinValue),
                    v => DateOnly.FromDateTime(v));
        });
    }
}
