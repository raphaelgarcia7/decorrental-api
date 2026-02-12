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

        var reservation = kit.Reservations.FirstOrDefault(r => r.Id == command.ReservationId)
            ?? throw new NotFoundException("Reservation not found.");

        reservation.Cancel();
        await _repository.SaveAsync(kit, cancellationToken);
    }
}
