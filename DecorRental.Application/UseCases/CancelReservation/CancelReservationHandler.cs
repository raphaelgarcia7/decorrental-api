using DecorRental.Application.Exceptions;
using DecorRental.Application.IntegrationEvents;
using DecorRental.Application.Messaging;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.CancelReservation;

public sealed class CancelReservationHandler
{
    private readonly IKitThemeRepository _repository;
    private readonly IMessageBus _messageBus;

    public CancelReservationHandler(IKitThemeRepository repository, IMessageBus messageBus)
    {
        _repository = repository;
        _messageBus = messageBus;
    }

    public async Task<CancelReservationResult> HandleAsync(
        CancelReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var kitTheme = await _repository.GetByIdAsync(command.KitThemeId, cancellationToken)
            ?? throw new NotFoundException("Tema de kit nao encontrado.");

        var hasReservation = kitTheme.Reservations.Any(reservation => reservation.Id == command.ReservationId);
        if (!hasReservation)
        {
            throw new NotFoundException("Reserva nao encontrada.");
        }

        var reservation = kitTheme.CancelReservation(command.ReservationId);
        await _repository.SaveAsync(kitTheme, cancellationToken);

        var integrationEvent = new ReservationCancelledEvent(
            kitTheme.Id,
            reservation.KitCategoryId,
            reservation.Id,
            reservation.Status.ToString());

        await _messageBus.PublishAsync(integrationEvent, cancellationToken);

        return new CancelReservationResult(
            reservation.Id,
            kitTheme.Id,
            reservation.Status.ToString());
    }
}
