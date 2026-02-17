using DecorRental.Application.IntegrationEvents;
using DecorRental.Application.Exceptions;
using DecorRental.Application.UseCases.CancelReservation;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Enums;
using DecorRental.Domain.ValueObjects;
using DecorRental.Tests.Application.Fakes;
using Xunit;

namespace DecorRental.Tests.Application;

public class CancelReservationTests
{
    [Fact]
    public async Task Should_cancel_reservation_when_reservation_exists()
    {
        var kit = new Kit("Basic Kit");
        kit.Reserve(new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 12)));

        var reservationId = kit.Reservations.Single().Id;

        var repository = new FakeKitRepository();
        var messageBus = new FakeMessageBus();
        await repository.AddAsync(kit);

        var handler = new CancelReservationHandler(repository, messageBus);

        var result = await handler.HandleAsync(new CancelReservationCommand(kit.Id, reservationId));

        var cancelledReservation = kit.Reservations.Single(reservation => reservation.Id == reservationId);
        Assert.Equal(ReservationStatus.Cancelled, cancelledReservation.Status);
        Assert.Equal(reservationId, result.ReservationId);
        Assert.Equal("Cancelled", result.ReservationStatus);
        Assert.Single(messageBus.PublishedEvents.OfType<ReservationCancelledEvent>());
    }

    [Fact]
    public async Task Should_throw_not_found_when_reservation_does_not_exist()
    {
        var kit = new Kit("Basic Kit");

        var repository = new FakeKitRepository();
        var messageBus = new FakeMessageBus();
        await repository.AddAsync(kit);

        var handler = new CancelReservationHandler(repository, messageBus);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.HandleAsync(new CancelReservationCommand(kit.Id, Guid.NewGuid())));
    }
}
