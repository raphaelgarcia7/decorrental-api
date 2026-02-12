using DecorRental.Application.Exceptions;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.GetKitById;

public sealed class GetKitByIdHandler
{
    private readonly IKitRepository _repository;

    public GetKitByIdHandler(IKitRepository repository)
    {
        _repository = repository;
    }

    public async Task<Kit> HandleAsync(GetKitByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(query.KitId, cancellationToken)
            ?? throw new NotFoundException("Kit not found.");
    }
}
