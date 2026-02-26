using DecorRental.Domain.Entities;
using DecorRental.Domain.Enums;
using DecorRental.Domain.ValueObjects;
using Xunit;

namespace DecorRental.Tests.Entities;

public class ReservationTests
{
    [Fact]
    public void Reservation_should_start_as_active()
    {
        var itemType = new ItemType("Panel", 10);
        var category = new KitCategory("Basic");
        category.AddOrUpdateItem(itemType.Id, 2);
        var period = new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 12));

        var reservation = Reservation.Create(
            Guid.NewGuid(),
            category,
            period,
            false,
            null,
            "Maria Silva",
            "12345678900",
            "Rua das Flores, 123",
            "Cliente recorrente.",
            true);

        Assert.Equal(ReservationStatus.Active, reservation.Status);
        Assert.Single(reservation.Items);
        Assert.Equal("Maria Silva", reservation.CustomerName);
        Assert.Equal("12345678900", reservation.CustomerDocumentNumber);
        Assert.Equal("Rua das Flores, 123", reservation.CustomerAddress);
        Assert.Equal("Cliente recorrente.", reservation.Notes);
        Assert.True(reservation.HasBalloonArch);
    }

    [Fact]
    public void Cancel_should_change_status_to_cancelled()
    {
        var itemType = new ItemType("Table", 10);
        var category = new KitCategory("Basic");
        category.AddOrUpdateItem(itemType.Id, 1);
        var period = new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 12));

        var reservation = Reservation.Create(
            Guid.NewGuid(),
            category,
            period,
            false,
            null,
            "Joao Souza",
            "98765432100",
            "Av. Brasil, 456",
            null,
            false);

        reservation.Cancel();

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }

    [Fact]
    public void Reservation_should_store_override_reason_when_enabled()
    {
        var itemType = new ItemType("Table", 2);
        var category = new KitCategory("Basic");
        category.AddOrUpdateItem(itemType.Id, 1);
        var period = new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 12));

        var reservation = Reservation.Create(
            Guid.NewGuid(),
            category,
            period,
            true,
            "Aprovacao manual para excecao.",
            "Ana Lima",
            "11222333444",
            "Rua Central, 789",
            "Reserva com falta parcial de itens.",
            false);

        Assert.True(reservation.IsStockOverride);
        Assert.Equal("Aprovacao manual para excecao.", reservation.StockOverrideReason);
        Assert.Equal("Ana Lima", reservation.CustomerName);
        Assert.Equal("11222333444", reservation.CustomerDocumentNumber);
        Assert.Equal("Rua Central, 789", reservation.CustomerAddress);
        Assert.Equal("Reserva com falta parcial de itens.", reservation.Notes);
        Assert.False(reservation.HasBalloonArch);
    }
}
