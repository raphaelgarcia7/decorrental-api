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
            throw new DomainException("O nome do tipo de item e obrigatorio.");
        }

        if (totalStock < 0)
        {
            throw new DomainException("O estoque total deve ser zero ou maior.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
        TotalStock = totalStock;
    }

    public void UpdateStock(int totalStock)
    {
        if (totalStock < 0)
        {
            throw new DomainException("O estoque total deve ser zero ou maior.");
        }

        TotalStock = totalStock;
    }

    private ItemType() { }
}
