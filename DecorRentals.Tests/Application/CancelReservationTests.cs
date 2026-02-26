using DecorRental.Application.Exceptions;
using DecorRental.Application.IntegrationEvents;
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
        var category = new KitCategory("Basic");
        var itemType = new ItemType("Table", 10);
        category.AddOrUpdateItem(itemType.Id, 1);

        var kitTheme = new KitTheme("Patrol Theme");
        var reservation = kitTheme.Reserve(
            category,
            new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 12)),
            false,
            null,
            "Cliente Teste",
            "12345678900",
            "Rua Teste, 100",
            null,
            false);

        var repository = new FakeKitThemeRepository();
        var messageBus = new FakeMessageBus();
        await repository.AddAsync(kitTheme);

        var handler = new CancelReservationHandler(repository, messageBus);

        var result = await handler.HandleAsync(new CancelReservationCommand(kitTheme.Id, reservation.Id));

        var cancelledReservation = kitTheme.Reservations.Single(currentReservation => currentReservation.Id == reservation.Id);
        Assert.Equal(ReservationStatus.Cancelled, cancelledReservation.Status);
        Assert.Equal(reservation.Id, result.ReservationId);
        Assert.Equal("Cancelled", result.ReservationStatus);
        Assert.Single(messageBus.PublishedEvents.OfType<ReservationCancelledEvent>());
    }

    [Fact]
    public async Task Should_throw_not_found_when_reservation_does_not_exist()
    {
        var kitTheme = new KitTheme("Basic Theme");

        var repository = new FakeKitThemeRepository();
        var messageBus = new FakeMessageBus();
        await repository.AddAsync(kitTheme);

        var handler = new CancelReservationHandler(repository, messageBus);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.HandleAsync(new CancelReservationCommand(kitTheme.Id, Guid.NewGuid())));
    }
}
