using DecorRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DecorRental.Infrastructure.Configurations;

public sealed class KitThemeConfiguration : IEntityTypeConfiguration<KitTheme>
{
    public void Configure(EntityTypeBuilder<KitTheme> builder)
    {
        builder.ToTable("KitThemes");
        builder.HasKey(kitTheme => kitTheme.Id);

        builder.Property(kitTheme => kitTheme.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasMany(kitTheme => kitTheme.Reservations)
            .WithOne()
            .HasForeignKey(reservation => reservation.KitThemeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(kitTheme => kitTheme.Reservations)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
