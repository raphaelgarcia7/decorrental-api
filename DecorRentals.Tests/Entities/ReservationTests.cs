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
        var period = new DateRange(
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12));

        var reservation = new Reservation(Guid.NewGuid(), period);

        Assert.Equal(ReservationStatus.Active, reservation.Status);
    }

    [Fact]
    public void Cancel_should_change_status_to_cancelled()
    {
        var period = new DateRange(
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12));

        var reservation = new Reservation(Guid.NewGuid(), period);

        reservation.Cancel();

        Assert.Equal(ReservationStatus.Cancelled, reservation.Status);
    }
}
