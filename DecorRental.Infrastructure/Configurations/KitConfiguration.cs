using DecorRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DecorRental.Infrastructure.Configurations;

public sealed class KitConfiguration : IEntityTypeConfiguration<Kit>
{
    public void Configure(EntityTypeBuilder<Kit> builder)
    {
        builder.HasKey(kit => kit.Id);

        builder.Property(kit => kit.Name)
            .IsRequired()
            .HasMaxLength(200);

        // Diz ao EF que a coleção real está no campo privado "_reservations"
        builder.HasMany<Reservation>("_reservations")
            .WithOne()
            .HasForeignKey("KitId")
            .OnDelete(DeleteBehavior.Cascade);

        // Garante que o EF use o campo em vez da propriedade read-only
        builder.Navigation(k => k.Reservations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
