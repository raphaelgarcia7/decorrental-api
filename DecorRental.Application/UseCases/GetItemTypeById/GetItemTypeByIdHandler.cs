using DecorRental.Application.Exceptions;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.GetItemTypeById;

public sealed class GetItemTypeByIdHandler
{
    private readonly IItemTypeRepository _repository;

    public GetItemTypeByIdHandler(IItemTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<ItemType> HandleAsync(Guid itemTypeId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(itemTypeId, cancellationToken)
            ?? throw new NotFoundException("Item type not found.");
    }
}
