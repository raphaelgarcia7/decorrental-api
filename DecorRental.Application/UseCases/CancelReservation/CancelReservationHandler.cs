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

    public void Handle(CancelReservationCommand command)
    {
        var kit = _repository.GetById(command.KitId)
            ?? throw new NotFoundException("Kit not found.");

        var reservation = kit.Reservations.FirstOrDefault(r => r.Id == command.ReservationId)
            ?? throw new NotFoundException("Reservation not found.");

        reservation.Cancel();
        _repository.Save(kit);
    }
}
