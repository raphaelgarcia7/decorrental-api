using DecorRental.Application.UseCases.ReserveKit;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Exceptions;
using DecorRental.Tests.Application.Fakes;
using Xunit;

namespace DecorRental.Tests.Application;

public class ReserveKitTests
{
    [Fact]
    public async Task Should_reserve_kit_when_period_is_available()
    {
        var kit = new Kit("Basic Kit");

        var repository = new FakeKitRepository();
        await repository.AddAsync(kit);

        var command = new ReserveKitCommand(
            kit.Id,
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12));

        var handler = new ReserveKitHandler(repository);

        var result = await handler.HandleAsync(command);

        Assert.Single(kit.Reservations);
        Assert.Equal(kit.Id, result.KitId);
        Assert.Equal("Active", result.ReservationStatus);
    }

    [Fact]
    public async Task Should_throw_exception_when_period_conflicts()
    {
        var kit = new Kit("Basic Kit");

        var repository = new FakeKitRepository();
        await repository.AddAsync(kit);

        var handler = new ReserveKitHandler(repository);

        await handler.HandleAsync(new ReserveKitCommand(
            kit.Id,
            new DateOnly(2026, 1, 10),
            new DateOnly(2026, 1, 12)));

        await Assert.ThrowsAsync<ConflictException>(async () =>
            await handler.HandleAsync(new ReserveKitCommand(
                kit.Id,
                new DateOnly(2026, 1, 11),
                new DateOnly(2026, 1, 13))));
    }
}
