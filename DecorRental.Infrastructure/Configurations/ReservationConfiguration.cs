using DecorRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DecorRental.Infrastructure.Configurations;

public sealed class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations");
        builder.HasKey(reservation => reservation.Id);

        builder.Property(reservation => reservation.Status)
            .HasConversion<int>();

        builder.HasOne<KitCategory>()
            .WithMany()
            .HasForeignKey(reservation => reservation.KitCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.OwnsOne(reservation => reservation.Period, period =>
        {
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

        builder.HasMany(reservation => reservation.Items)
            .WithOne()
            .HasForeignKey(reservationItem => reservationItem.ReservationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(reservation => reservation.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(reservation => new { reservation.Status, reservation.KitThemeId });
    }
}
