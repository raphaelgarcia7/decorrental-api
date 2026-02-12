using DecorRental.Application.Exceptions;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Application.UseCases.ReserveKit;

public class ReserveKitHandler
{

    private readonly IKitRepository _repository;

    public ReserveKitHandler(IKitRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(ReserveKitCommand reserveKitCommand, CancellationToken cancellationToken = default)
    {
        var kit = await _repository.GetByIdAsync(reserveKitCommand.KitId, cancellationToken)
            ?? throw new NotFoundException("Kit not found.");

        var period = new DateRange(
            reserveKitCommand.StartDate,
            reserveKitCommand.EndDate);

        kit.Reserve(period);

        await _repository.SaveAsync(kit, cancellationToken);
    }
}
