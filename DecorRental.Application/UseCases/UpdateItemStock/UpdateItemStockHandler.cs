using DecorRental.Application.Exceptions;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.UpdateItemStock;

public sealed class UpdateItemStockHandler
{
    private readonly IItemTypeRepository _repository;

    public UpdateItemStockHandler(IItemTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ItemType> HandleAsync(UpdateItemStockCommand command, CancellationToken cancellationToken = default)
    {
        var itemType = await _repository.GetByIdAsync(command.ItemTypeId, cancellationToken)
            ?? throw new NotFoundException("Tipo de item nao encontrado.");

        itemType.UpdateStock(command.TotalStock);
        await _repository.SaveAsync(itemType, cancellationToken);

        return itemType;
    }
}
