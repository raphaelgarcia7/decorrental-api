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
            new DateOnly(2026, 1, 12),
            false,
            null,
            "Gabriela Costa",
            "11122233344",
            "12988887777",
            "Rua das Palmeiras, 88",
            "Entrega em condominio.",
            true,
            true);

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
        Assert.False(result.IsStockOverride);
        Assert.Null(result.StockOverrideReason);
        Assert.Equal("Gabriela Costa", result.CustomerName);
        Assert.Equal("11122233344", result.CustomerDocumentNumber);
        Assert.Equal("12988887777", result.CustomerPhoneNumber);
        Assert.Equal("Rua das Palmeiras, 88", result.CustomerAddress);
        Assert.Equal("Entrega em condominio.", result.Notes);
        Assert.True(result.HasBalloonArch);
        Assert.True(result.IsEntryPaid);
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
            Guid.NewGuid(),
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
            new DateOnly(2026, 1, 11),
            new DateOnly(2026, 1, 14),
            false,
            null,
            "Ricardo Souza",
            "99888777666",
            "11998887766",
            "Rua Augusta, 100",
            null,
            false,
            false);

        var handler = new ReserveKitHandler(
            kitThemeRepository,
            categoryRepository,
            itemTypeRepository,
            reservationQueryRepository,
            messageBus);

        await Assert.ThrowsAsync<ConflictException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task Should_allow_reserve_with_stock_override_when_reason_is_informed()
    {
        var itemType = new ItemType("Arch", 1);
        var category = new KitCategory("Basic");
        category.AddOrUpdateItem(itemType.Id, 1);
        var kitTheme = new KitTheme("Theme");

        var kitThemeRepository = new FakeKitThemeRepository();
        var categoryRepository = new FakeKitCategoryRepository();
        var itemTypeRepository = new FakeItemTypeRepository();
        var reservationQueryRepository = new FakeReservationQueryRepository();
        var messageBus = new FakeMessageBus();

        reservationQueryRepository.Add(new ActiveReservationItem(
            Guid.NewGuid(),
            itemType.Id,
            1,
            new DateOnly(2026, 2, 22),
            new DateOnly(2026, 2, 24)));

        await kitThemeRepository.AddAsync(kitTheme);
        await categoryRepository.AddAsync(category);
        await itemTypeRepository.AddAsync(itemType);

        var command = new ReserveKitCommand(
            kitTheme.Id,
            category.Id,
            new DateOnly(2026, 2, 23),
            new DateOnly(2026, 2, 24),
            true,
            "Cliente recorrente aprovado pela operacao.",
            "Monica Santos",
            "12398745600",
            "21991234567",
            "Rua C, 400",
            "Montagem no fim da tarde.",
            false,
            false);

        var handler = new ReserveKitHandler(
            kitThemeRepository,
            categoryRepository,
            itemTypeRepository,
            reservationQueryRepository,
            messageBus);

        var result = await handler.HandleAsync(command);

        Assert.True(result.IsStockOverride);
        Assert.Equal("Cliente recorrente aprovado pela operacao.", result.StockOverrideReason);
    }
}
