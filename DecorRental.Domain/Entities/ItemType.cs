using DecorRental.Domain.Exceptions;

namespace DecorRental.Domain.Entities;

public class ItemType
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public int TotalStock { get; private set; }

    public ItemType(string name, int totalStock)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Item type name is required.");
        }

        if (totalStock < 0)
        {
            throw new DomainException("Total stock must be zero or greater.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        TotalStock = totalStock;
    }

    public void UpdateStock(int totalStock)
    {
        if (totalStock < 0)
        {
            throw new DomainException("Total stock must be zero or greater.");
        }

        TotalStock = totalStock;
    }

    private ItemType() { }
}
