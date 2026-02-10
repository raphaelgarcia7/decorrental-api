using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.GetKits;

public sealed class GetKitsHandler
{
    private readonly IKitRepository _repository;

    public GetKitsHandler(IKitRepository repository)
    {
        _repository = repository;
    }

    public IReadOnlyList<Kit> Handle(GetKitsQuery query)
    {
        return _repository.GetAll();
    }
}
