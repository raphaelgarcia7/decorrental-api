using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.CreateItemType;

public sealed class CreateItemTypeHandler
{
    private readonly IItemTypeRepository _repository;

    public CreateItemTypeHandler(IItemTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> HandleAsync(CreateItemTypeCommand command, CancellationToken cancellationToken = default)
    {
        var itemType = new ItemType(command.Name, command.TotalStock);
        await _repository.AddAsync(itemType, cancellationToken);

        return itemType.Id;
    }
}
