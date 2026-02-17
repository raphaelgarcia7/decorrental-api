using DecorRental.Application.Exceptions;
using DecorRental.Application.IntegrationEvents;
using DecorRental.Application.Messaging;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Application.UseCases.ReserveKit;

public class ReserveKitHandler
{

    private readonly IKitRepository _repository;
    private readonly IMessageBus _messageBus;

    public ReserveKitHandler(IKitRepository repository, IMessageBus messageBus)
    {
        _repository = repository;
        _messageBus = messageBus;
    }

    public async Task<ReserveKitResult> HandleAsync(
        ReserveKitCommand reserveKitCommand,
        CancellationToken cancellationToken = default)
    {
        var kit = await _repository.GetByIdAsync(reserveKitCommand.KitId, cancellationToken)
            ?? throw new NotFoundException("Kit not found.");

        var period = new DateRange(
            reserveKitCommand.StartDate,
            reserveKitCommand.EndDate);

        var reservation = kit.Reserve(period);

        await _repository.SaveAsync(kit, cancellationToken);

        var integrationEvent = new ReservationCreatedEvent(
            kit.Id,
            reservation.Id,
            reservation.Period.Start,
            reservation.Period.End,
            reservation.Status.ToString());

        await _messageBus.PublishAsync(integrationEvent, cancellationToken);

        return new ReserveKitResult(
            reservation.Id,
            kit.Id,
            reservation.Period.Start,
            reservation.Period.End,
            reservation.Status.ToString());
    }
}
