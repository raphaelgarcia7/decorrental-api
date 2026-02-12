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

        // Usa a propriedade de navegação, mas acessa via backing field
        builder.HasMany(kit => kit.Reservations)
            .WithOne()
            .HasForeignKey("KitId")
            .OnDelete(DeleteBehavior.Cascade);

        // Garante que o EF use o campo privado em vez da propriedade read-only
        builder.Navigation(kit => kit.Reservations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
