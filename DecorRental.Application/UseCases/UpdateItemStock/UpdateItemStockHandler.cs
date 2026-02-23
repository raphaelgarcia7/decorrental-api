using DecorRental.Application.Exceptions;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.UpdateItemStock;

public sealed class UpdateItemStockHandler
{
    private readonly IItemTypeRepository _repository;

    public UpdateItemStockHandler(IItemTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(UpdateItemStockCommand command, CancellationToken cancellationToken = default)
    {
        var itemType = await _repository.GetByIdAsync(command.ItemTypeId, cancellationToken)
            ?? throw new NotFoundException("Item type not found.");

        itemType.UpdateStock(command.TotalStock);
        await _repository.SaveAsync(itemType, cancellationToken);
    }
}
