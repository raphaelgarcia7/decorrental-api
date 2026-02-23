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

        var reservation = Reservation.Create(Guid.NewGuid(), category, period);

        Assert.Equal(ReservationStatus.Active, reservation.Status);
        Assert.Single(reservation.Items);
    }

    [Fact]
    public void Cancel_should_change_status_to_cancelled()
    {
        var itemType = new ItemType("Table", 10);
        var category = new KitCategory("Basic");
        category.AddOrUpdateItem(itemType.Id, 1);
        var period = new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 12));

        var reservation = Reservation.Create(Guid.NewGuid(), category, period);

        reservation.Cancel();

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }
}
