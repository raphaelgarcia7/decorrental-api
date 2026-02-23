using DecorRental.Domain.Exceptions;

namespace DecorRental.Domain.Entities;

public class CategoryItem
{
    public Guid Id { get; private set; }
    public Guid KitCategoryId { get; private set; }
    public Guid ItemTypeId { get; private set; }
    public int Quantity { get; private set; }

    public CategoryItem(Guid kitCategoryId, Guid itemTypeId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Category item quantity must be greater than zero.");
        }

        Id = Guid.NewGuid();
        KitCategoryId = kitCategoryId;
        ItemTypeId = itemTypeId;
        Quantity = quantity;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("Category item quantity must be greater than zero.");
        }

        Quantity = quantity;
    }

    private CategoryItem() { }
}
