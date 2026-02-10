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

    public Kit Handle(GetKitByIdQuery query)
    {
        return _repository.GetById(query.KitId)
            ?? throw new NotFoundException("Kit not found.");
    }
}
