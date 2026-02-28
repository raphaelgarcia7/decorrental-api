using DecorRental.Application.UseCases.UpdateReservation;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;
using DecorRental.Domain.ValueObjects;
using DecorRental.Tests.Application.Fakes;
using Xunit;

namespace DecorRental.Tests.Application;

public sealed class UpdateReservationTests
{
    [Fact]
    public async Task Should_update_reservation_when_current_reservation_is_ignored_from_stock_check()
    {
        var itemType = new ItemType("Painel Romano", 1);
        var category = new KitCategory("Basica");
        category.AddOrUpdateItem(itemType.Id, 1);
        var kitTheme = new KitTheme("Turma da Monica");
        var originalReservation = kitTheme.Reserve(
            category,
            new DateRange(new DateOnly(2026, 7, 10), new DateOnly(2026, 7, 12)),
            false,
            null,
            "Cliente Original",
            "12345678900",
            "12999990000",
            "Rua A, 100",
            null,
            false,
            false);

        var kitThemeRepository = new FakeKitThemeRepository();
        var categoryRepository = new FakeKitCategoryRepository();
        var itemTypeRepository = new FakeItemTypeRepository();
        var reservationQueryRepository = new FakeReservationQueryRepository();
        var messageBus = new FakeMessageBus();

        await kitThemeRepository.AddAsync(kitTheme);
        await categoryRepository.AddAsync(category);
        await itemTypeRepository.AddAsync(itemType);

        reservationQueryRepository.Add(new ActiveReservationItem(
            originalReservation.Id,
            itemType.Id,
            1,
            new DateOnly(2026, 7, 10),
            new DateOnly(2026, 7, 12)));

        var command = new UpdateReservationCommand(
            kitTheme.Id,
            originalReservation.Id,
            category.Id,
            new DateOnly(2026, 7, 11),
            new DateOnly(2026, 7, 13),
            false,
            null,
            "Cliente Atualizado",
            "99888777666",
            "11998887766",
            "Rua B, 200",
            "Atualizacao manual.",
            true,
            true);

        var handler = new UpdateReservationHandler(
            kitThemeRepository,
            categoryRepository,
            itemTypeRepository,
            reservationQueryRepository,
            messageBus);

        var result = await handler.HandleAsync(command);

        Assert.Equal(originalReservation.Id, result.ReservationId);
        Assert.Equal("Cliente Atualizado", result.CustomerName);
        Assert.Equal("99888777666", result.CustomerDocumentNumber);
        Assert.Equal("11998887766", result.CustomerPhoneNumber);
        Assert.Equal("Rua B, 200", result.CustomerAddress);
        Assert.Equal("Atualizacao manual.", result.Notes);
        Assert.True(result.HasBalloonArch);
        Assert.True(result.IsEntryPaid);
        Assert.False(result.IsStockOverride);
    }
}
