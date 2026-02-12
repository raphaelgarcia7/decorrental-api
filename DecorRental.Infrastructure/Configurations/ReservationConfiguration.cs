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
            period.Property(dateRange => dateRange.Start)
                .HasColumnName("StartDate")
                .HasConversion(
                    date => date.ToDateTime(TimeOnly.MinValue),
                    dateTime => DateOnly.FromDateTime(dateTime));

            period.Property(dateRange => dateRange.End)
                .HasColumnName("EndDate")
                .HasConversion(
                    date => date.ToDateTime(TimeOnly.MinValue),
                    dateTime => DateOnly.FromDateTime(dateTime));
        });
    }
}
