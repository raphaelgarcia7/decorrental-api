using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.GetItemTypes;

public sealed class GetItemTypesHandler
{
    private readonly IItemTypeRepository _repository;

    public GetItemTypesHandler(IItemTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetItemTypesResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var items = await _repository.GetAllAsync(cancellationToken);
        return new GetItemTypesResult(items);
    }
}
