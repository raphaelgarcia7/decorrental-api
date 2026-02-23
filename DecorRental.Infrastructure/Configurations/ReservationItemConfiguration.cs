using DecorRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DecorRental.Infrastructure.Configurations;

public sealed class ReservationItemConfiguration : IEntityTypeConfiguration<ReservationItem>
{
    public void Configure(EntityTypeBuilder<ReservationItem> builder)
    {
        builder.ToTable("ReservationItems");
        builder.HasKey(reservationItem => reservationItem.Id);

        builder.Property(reservationItem => reservationItem.Quantity)
            .IsRequired();

        builder.HasOne<ItemType>()
            .WithMany()
            .HasForeignKey(reservationItem => reservationItem.ItemTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(reservationItem => reservationItem.ItemTypeId);
    }
}
