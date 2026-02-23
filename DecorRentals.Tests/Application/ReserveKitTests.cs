using DecorRental.Application.IntegrationEvents;
using DecorRental.Application.UseCases.ReserveKit;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Exceptions;
using DecorRental.Domain.Repositories;
using DecorRental.Tests.Application.Fakes;
using Xunit;

namespace DecorRental.Tests.Application;

public class ReserveKitTests
{
    [Fact]
    public async Task Should_reserve_kit_theme_when_stock_is_available()
    {
        var itemType = new ItemType("Balloon", 10);
        var category = new KitCategory("Basic");
        category.AddOrUpdateItem(itemType.Id, 3);
        var kitTheme = new KitTheme("Patrol Theme");

        var kitThemeRepository = new FakeKitThemeRepository();
        var categoryRepository = new FakeKitCategoryRepository();
        var itemTypeRepository = new FakeItemTypeRepository();
        var reservationQueryRepository = new FakeReservationQueryRepository();
        var messageBus = new FakeMessageBus();

        await kitThemeRepository.AddAsync(kitTheme);
        await categoryRepository.AddAsync(category);
        await itemTypeRepository.AddAsync(itemType);

        var command = new ReserveKitCommand(
            kitTheme.Id,
            category.Id,
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12));

        var handler = new ReserveKitHandler(
            kitThemeRepository,
            categoryRepository,
            itemTypeRepository,
            reservationQueryRepository,
            messageBus);

        var result = await handler.HandleAsync(command);

        Assert.Single(kitTheme.Reservations);
        Assert.Equal(kitTheme.Id, result.KitThemeId);
        Assert.Equal(category.Id, result.KitCategoryId);
        Assert.Equal("Active", result.ReservationStatus);
        Assert.Single(messageBus.PublishedEvents.OfType<ReservationCreatedEvent>());
    }

    [Fact]
    public async Task Should_throw_conflict_when_stock_is_insufficient_in_overlapping_period()
    {
        var itemType = new ItemType("Panel", 5);
        var category = new KitCategory("Complete");
        category.AddOrUpdateItem(itemType.Id, 2);
        var kitTheme = new KitTheme("Paw Theme");

        var kitThemeRepository = new FakeKitThemeRepository();
        var categoryRepository = new FakeKitCategoryRepository();
        var itemTypeRepository = new FakeItemTypeRepository();
        var reservationQueryRepository = new FakeReservationQueryRepository();
        var messageBus = new FakeMessageBus();

        reservationQueryRepository.Add(new ActiveReservationItem(
            itemType.Id,
            4,
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12)));

        await kitThemeRepository.AddAsync(kitTheme);
        await categoryRepository.AddAsync(category);
        await itemTypeRepository.AddAsync(itemType);

        var command = new ReserveKitCommand(
            kitTheme.Id,
            category.Id,
            new DateOnly(2026, 1, 12),
            new DateOnly(2026, 1, 14));

        var handler = new ReserveKitHandler(
            kitThemeRepository,
            categoryRepository,
            itemTypeRepository,
            reservationQueryRepository,
            messageBus);

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(command));
    }
}
