using DecorRental.Application.Exceptions;
using DecorRental.Application.IntegrationEvents;
using DecorRental.Application.Messaging;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.CancelReservation;

public sealed class CancelReservationHandler
{
    private readonly IKitRepository _repository;
    private readonly IMessageBus _messageBus;

    public CancelReservationHandler(IKitRepository repository, IMessageBus messageBus)
    {
        _repository = repository;
        _messageBus = messageBus;
    }

    public async Task<CancelReservationResult> HandleAsync(
        CancelReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var kit = await _repository.GetByIdAsync(command.KitId, cancellationToken)
            ?? throw new NotFoundException("Kit not found.");

        var hasReservation = kit.Reservations.Any(reservation => reservation.Id == command.ReservationId);
        if (!hasReservation)
        {
            throw new NotFoundException("Reservation not found.");
        }

        var reservation = kit.CancelReservation(command.ReservationId);
        await _repository.SaveAsync(kit, cancellationToken);

        var integrationEvent = new ReservationCancelledEvent(
            kit.Id,
            reservation.Id,
            reservation.Status.ToString());

        await _messageBus.PublishAsync(integrationEvent, cancellationToken);

        return new CancelReservationResult(
            reservation.Id,
            kit.Id,
            reservation.Status.ToString());
    }
}
