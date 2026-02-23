using DecorRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DecorRental.Infrastructure.Configurations;

public sealed class CategoryItemConfiguration : IEntityTypeConfiguration<CategoryItem>
{
    public void Configure(EntityTypeBuilder<CategoryItem> builder)
    {
        builder.ToTable("CategoryItems");
        builder.HasKey(categoryItem => categoryItem.Id);

        builder.Property(categoryItem => categoryItem.Quantity)
            .IsRequired();

        builder.HasIndex(categoryItem => new { categoryItem.KitCategoryId, categoryItem.ItemTypeId })
            .IsUnique();

        builder.HasOne<ItemType>()
            .WithMany()
            .HasForeignKey(categoryItem => categoryItem.ItemTypeId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
