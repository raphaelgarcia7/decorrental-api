using DecorRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DecorRental.Infrastructure.Configurations;

public sealed class ItemTypeConfiguration : IEntityTypeConfiguration<ItemType>
{
    public void Configure(EntityTypeBuilder<ItemType> builder)
    {
        builder.ToTable("ItemTypes");
        builder.HasKey(itemType => itemType.Id);

        builder.Property(itemType => itemType.Name)
            .IsRequired()
            .HasMaxLength(120);

        builder.HasIndex(itemType => itemType.Name)
            .IsUnique();

        builder.Property(itemType => itemType.TotalStock)
            .IsRequired();
    }
}
