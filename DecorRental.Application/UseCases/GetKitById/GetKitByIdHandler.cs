using DecorRental.Application.Exceptions;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.GetKitById;

public sealed class GetKitByIdHandler
{
    private readonly IKitThemeRepository _repository;

    public GetKitByIdHandler(IKitThemeRepository repository)
    {
        _repository = repository;
    }

    public async Task<KitTheme> HandleAsync(GetKitByIdQuery query, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(query.KitThemeId, cancellationToken)
            ?? throw new NotFoundException("Tema de kit nao encontrado.");
    }
}
