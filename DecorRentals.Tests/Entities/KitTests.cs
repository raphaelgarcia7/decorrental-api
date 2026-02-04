using DecorRental.Domain.Entities;
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

        Assert.Throws<DomainException>(() =>
            kit.Reserve(new DateRange(
                new DateOnly(2026, 1, 11),
                new DateOnly(2026, 1, 13))));
    }
}
