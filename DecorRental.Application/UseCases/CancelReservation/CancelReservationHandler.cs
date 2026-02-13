using DecorRental.Application.Exceptions;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.CancelReservation;

public sealed class CancelReservationHandler
{
    private readonly IKitRepository _repository;

    public CancelReservationHandler(IKitRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(CancelReservationCommand command, CancellationToken cancellationToken = default)
    {
        var kit = await _repository.GetByIdAsync(command.KitId, cancellationToken)
            ?? throw new NotFoundException("Kit not found.");

        var hasReservation = kit.Reservations.Any(reservation => reservation.Id == command.ReservationId);
        if (!hasReservation)
        {
            throw new NotFoundException("Reservation not found.");
        }

        kit.CancelReservation(command.ReservationId);
        await _repository.SaveAsync(kit, cancellationToken);
    }
}
