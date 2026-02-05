using DecorRental.Application.UseCases.ReserveKit;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Exceptions;
using Xunit;

namespace DecorRental.Tests.Application;

public class ReserveKitTests
{
    [Fact]
    public void Should_reserve_kit_when_period_is_available()
    {
        var kit = new Kit("Basic Kit");

        var command = new ReserveKitCommand(
            kit.Id,
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12));

        var handler = new ReserveKitHandler();

        handler.Handle(kit, command);

        Assert.Single(kit.Reservations);
    }

    [Fact]
    public void Should_throw_exception_when_period_conflicts()
    {
        var kit = new Kit("Basic Kit");

        var handler = new ReserveKitHandler();

        handler.Handle(kit, new ReserveKitCommand(
            kit.Id,
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12)));

        Assert.Throws<DomainException>(() =>
            handler.Handle(kit, new ReserveKitCommand(
                kit.Id,
                new DateOnly(2026, 1, 11),
                new DateOnly(2026, 1, 13))));
    }
}
