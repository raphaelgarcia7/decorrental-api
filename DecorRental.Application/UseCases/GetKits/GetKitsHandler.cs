using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.GetKits;

public sealed class GetKitsHandler
{
    private readonly IKitRepository _repository;

    public GetKitsHandler(IKitRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetKitsResult> HandleAsync(GetKitsQuery query, CancellationToken cancellationToken = default)
    {
        var totalCount = await _repository.CountAsync(cancellationToken);
        var items = await _repository.GetPageAsync(query.Page, query.PageSize, cancellationToken);

        return new GetKitsResult(items, query.Page, query.PageSize, totalCount);
    }
}
