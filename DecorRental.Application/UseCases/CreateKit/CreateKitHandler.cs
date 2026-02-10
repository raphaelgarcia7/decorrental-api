using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.CreateKit;

public sealed class CreateKitHandler
{
    private readonly IKitRepository _repository;

    public CreateKitHandler(IKitRepository repository)
    {
        _repository = repository;
    }

    public Guid Handle(CreateKitCommand command)
    {
        var kit = new Kit(command.Name);
        _repository.Add(kit);

        return kit.Id;
    }
}
