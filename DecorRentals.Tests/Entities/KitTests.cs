using DecorRental.Domain.Entities;
using DecorRental.Domain.Enums;
using DecorRental.Domain.Exceptions;
using DecorRental.Domain.ValueObjects;
using Xunit;

namespace DecorRental.Tests.Entities;

public class KitTests
{
    [Fact]
    public void Should_not_allow_overlapping_reservations()
    {
        var kit = new Kit("Basic Kit");

        kit.Reserve(new DateRange(
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12)));

        Assert.Throws<ConflictException>(() =>
            kit.Reserve(new DateRange(
                new DateOnly(2026, 1, 11),
                new DateOnly(2026, 1, 13))));
    }

    [Fact]
    public void Should_allow_new_reservation_after_previous_reservation_is_cancelled()
    {
        var kit = new Kit("Basic Kit");
        var period = new DateRange(
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12));

        kit.Reserve(period);
        var reservationId = kit.Reservations.Single().Id;

        kit.CancelReservation(reservationId);
        kit.Reserve(period);

        Assert.Equal(2, kit.Reservations.Count);
        Assert.Single(kit.Reservations, reservation => reservation.Status == ReservationStatus.Active);
    }

    [Fact]
    public void Should_throw_when_cancel_reservation_does_not_exist()
    {
        var kit = new Kit("Basic Kit");

        Assert.Throws<DomainException>(() => kit.CancelReservation(Guid.NewGuid()));
    }
}
