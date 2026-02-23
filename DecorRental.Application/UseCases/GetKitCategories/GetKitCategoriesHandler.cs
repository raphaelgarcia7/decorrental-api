using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.GetKitCategories;

public sealed class GetKitCategoriesHandler
{
    private readonly IKitCategoryRepository _repository;

    public GetKitCategoriesHandler(IKitCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetKitCategoriesResult> HandleAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _repository.GetAllAsync(cancellationToken);
        return new GetKitCategoriesResult(categories);
    }
}
