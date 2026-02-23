using DecorRental.Domain.Exceptions;

namespace DecorRental.Domain.Entities;

public class KitCategory
{
    private readonly List<CategoryItem> _items = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public IReadOnlyCollection<CategoryItem> Items => _items;

    public KitCategory(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Category name is required.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
    }

    public void AddOrUpdateItem(Guid itemTypeId, int quantity)
    {
        var existingItem = _items.FirstOrDefault(categoryItem => categoryItem.ItemTypeId == itemTypeId);
        if (existingItem is null)
        {
            _items.Add(new CategoryItem(Id, itemTypeId, quantity));
            return;
        }

        existingItem.UpdateQuantity(quantity);
    }

    private KitCategory() { }
}
