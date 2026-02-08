using DecorRental.Application.UseCases.ReserveKit;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Exceptions;
using DecorRental.Tests.Application.Fakes;
using Xunit;

namespace DecorRental.Tests.Application;

public class ReserveKitTests
{
    [Fact]
    public void Should_reserve_kit_when_period_is_available()
    {
        var kit = new Kit("Basic Kit");

        var repository = new FakeKitRepository();
        repository.Add(kit);

        var command = new ReserveKitCommand(
            kit.Id,
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12));

        var handler = new ReserveKitHandler(repository);

        handler.Handle(command);

        Assert.Single(kit.Reservations);
    }

    [Fact]
    public void Should_throw_exception_when_period_conflicts()
    {
        var kit = new Kit("Basic Kit");

        var repository = new FakeKitRepository();
        repository.Add(kit);

        var handler = new ReserveKitHandler(repository);

        handler.Handle(new ReserveKitCommand(
            kit.Id,
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12)));

        Assert.Throws<DomainException>(() =>
            handler.Handle(new ReserveKitCommand(
                kit.Id,
                new DateOnly(2026, 1, 11),
                new DateOnly(2026, 1, 13))));
    }
}
