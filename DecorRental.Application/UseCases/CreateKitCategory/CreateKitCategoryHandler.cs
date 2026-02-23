using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.CreateKitCategory;

public sealed class CreateKitCategoryHandler
{
    private readonly IKitCategoryRepository _repository;

    public CreateKitCategoryHandler(IKitCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> HandleAsync(CreateKitCategoryCommand command, CancellationToken cancellationToken = default)
    {
        var category = new KitCategory(command.Name);
        await _repository.AddAsync(category, cancellationToken);

        return category.Id;
    }
}
