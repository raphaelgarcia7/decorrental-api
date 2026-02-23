using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.CreateKit;

public sealed class CreateKitHandler
{
    private readonly IKitThemeRepository _repository;

    public CreateKitHandler(IKitThemeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> HandleAsync(CreateKitCommand command, CancellationToken cancellationToken = default)
    {
        var kitTheme = new KitTheme(command.Name);
        await _repository.AddAsync(kitTheme, cancellationToken);

        return kitTheme.Id;
    }
}
