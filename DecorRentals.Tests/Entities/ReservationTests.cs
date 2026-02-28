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
            "12999998888",
            "Rua das Flores, 123",
            "Cliente recorrente.",
            true,
            true);

        Assert.Equal(ReservationStatus.Active, reservation.Status);
        Assert.Single(reservation.Items);
        Assert.Equal("Maria Silva", reservation.CustomerName);
        Assert.Equal("12345678900", reservation.CustomerDocumentNumber);
        Assert.Equal("12999998888", reservation.CustomerPhoneNumber);
        Assert.Equal("Rua das Flores, 123", reservation.CustomerAddress);
        Assert.Equal("Cliente recorrente.", reservation.Notes);
        Assert.True(reservation.HasBalloonArch);
        Assert.True(reservation.IsEntryPaid);
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
            "11998887766",
            "Av. Brasil, 456",
            null,
            false,
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
            "21997776655",
            "Rua Central, 789",
            "Reserva com falta parcial de itens.",
            false,
            false);

        Assert.True(reservation.IsStockOverride);
        Assert.Equal("Aprovacao manual para excecao.", reservation.StockOverrideReason);
        Assert.Equal("Ana Lima", reservation.CustomerName);
        Assert.Equal("11222333444", reservation.CustomerDocumentNumber);
        Assert.Equal("21997776655", reservation.CustomerPhoneNumber);
        Assert.Equal("Rua Central, 789", reservation.CustomerAddress);
        Assert.Equal("Reserva com falta parcial de itens.", reservation.Notes);
        Assert.False(reservation.HasBalloonArch);
        Assert.False(reservation.IsEntryPaid);
    }

    [Fact]
    public void Update_should_replace_customer_data_and_period()
    {
        var itemType = new ItemType("Painel", 3);
        var initialCategory = new KitCategory("Basica");
        initialCategory.AddOrUpdateItem(itemType.Id, 1);
        var updatedCategory = new KitCategory("Completa");
        updatedCategory.AddOrUpdateItem(itemType.Id, 2);
        var initialPeriod = new DateRange(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 2));

        var reservation = Reservation.Create(
            Guid.NewGuid(),
            initialCategory,
            initialPeriod,
            false,
            null,
            "Cliente Inicial",
            "12345678900",
            "11999998888",
            "Rua Inicial, 10",
            null,
            false,
            false);

        reservation.Update(
            updatedCategory,
            new DateRange(new DateOnly(2026, 8, 3), new DateOnly(2026, 8, 4)),
            true,
            "Aprovado manualmente.",
            "Cliente Atualizado",
            "99988877766",
            "11991112222",
            "Rua Atualizada, 20",
            "Obs atualizada.",
            true,
            true);

        Assert.Equal(updatedCategory.Id, reservation.KitCategoryId);
        Assert.Equal(new DateOnly(2026, 8, 3), reservation.Period.Start);
        Assert.Equal(new DateOnly(2026, 8, 4), reservation.Period.End);
        Assert.Equal("Cliente Atualizado", reservation.CustomerName);
        Assert.Equal("99988877766", reservation.CustomerDocumentNumber);
        Assert.Equal("11991112222", reservation.CustomerPhoneNumber);
        Assert.Equal("Rua Atualizada, 20", reservation.CustomerAddress);
        Assert.Equal("Obs atualizada.", reservation.Notes);
        Assert.True(reservation.IsStockOverride);
        Assert.Equal("Aprovado manualmente.", reservation.StockOverrideReason);
        Assert.Equal(2, reservation.Items.Single().Quantity);
    }
}
