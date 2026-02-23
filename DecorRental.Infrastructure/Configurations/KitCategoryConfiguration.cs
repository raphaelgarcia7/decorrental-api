using DecorRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DecorRental.Infrastructure.Configurations;

public sealed class KitCategoryConfiguration : IEntityTypeConfiguration<KitCategory>
{
    public void Configure(EntityTypeBuilder<KitCategory> builder)
    {
        builder.ToTable("KitCategories");
        builder.HasKey(category => category.Id);

        builder.Property(category => category.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.HasMany(category => category.Items)
            .WithOne()
            .HasForeignKey(categoryItem => categoryItem.KitCategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(category => category.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
