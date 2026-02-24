using DecorRental.Application.Exceptions;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.GetKitCategoryById;

public sealed class GetKitCategoryByIdHandler
{
    private readonly IKitCategoryRepository _repository;

    public GetKitCategoryByIdHandler(IKitCategoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<KitCategory> HandleAsync(Guid categoryId, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(categoryId, cancellationToken)
            ?? throw new NotFoundException("Categoria nao encontrada.");
    }
}
