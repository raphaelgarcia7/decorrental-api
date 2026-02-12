using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.GetKits;

public sealed class GetKitsHandler
{
    private readonly IKitRepository _repository;

    public GetKitsHandler(IKitRepository repository)
    {
        _repository = repository;
    }

    public GetKitsResult Handle(GetKitsQuery query)
    {
        var kits = _repository.GetAll();
        var totalCount = kits.Count;

        var items = kits
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToList();

        return new GetKitsResult(items, query.Page, query.PageSize, totalCount);
    }
}
