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

    public async Task<Guid> HandleAsync(CreateKitCommand command, CancellationToken cancellationToken = default)
    {
        var kit = new Kit(command.Name);
        await _repository.AddAsync(kit, cancellationToken);

        return kit.Id;
    }
}
