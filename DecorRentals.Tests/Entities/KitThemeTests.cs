using DecorRental.Domain.Entities;
using DecorRental.Domain.Enums;
using DecorRental.Domain.Exceptions;
using DecorRental.Domain.ValueObjects;
using Xunit;

namespace DecorRental.Tests.Entities;

public class KitThemeTests
{
    [Fact]
    public void Should_create_reservation_with_category_items_snapshot()
    {
        var itemType = new ItemType("Panel", 10);
        var category = new KitCategory("Basic");
        category.AddOrUpdateItem(itemType.Id, 2);

        var kitTheme = new KitTheme("Patrol Theme");
        var period = new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 12));

        var reservation = kitTheme.Reserve(category, period);

        Assert.Single(kitTheme.Reservations);
        Assert.Equal(category.Id, reservation.KitCategoryId);
        Assert.Single(reservation.Items);
        Assert.Equal(2, reservation.Items.Single().Quantity);
    }

    [Fact]
    public void Should_allow_new_reservation_after_previous_reservation_is_cancelled()
    {
        var itemType = new ItemType("Balloon", 100);
        var category = new KitCategory("Basic");
        category.AddOrUpdateItem(itemType.Id, 20);

        var kitTheme = new KitTheme("Patrol Theme");
        var period = new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 12));

        var reservation = kitTheme.Reserve(category, period);

        kitTheme.CancelReservation(reservation.Id);
        kitTheme.Reserve(category, period);

        Assert.Equal(2, kitTheme.Reservations.Count);
        Assert.Single(kitTheme.Reservations, currentReservation => currentReservation.Status == ReservationStatus.Active);
    }

    [Fact]
    public void Should_throw_when_cancel_reservation_does_not_exist()
    {
        var kitTheme = new KitTheme("Patrol Theme");

        Assert.Throws<DomainException>(() => kitTheme.CancelReservation(Guid.NewGuid()));
    }
}
