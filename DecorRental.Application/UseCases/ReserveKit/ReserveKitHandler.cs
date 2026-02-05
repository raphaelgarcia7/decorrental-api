using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Application.UseCases.ReserveKit;

public class ReserveKitHandler
{

    private readonly IKitRepository repository;

    public ReserveKitHandler(IKitRepository repository)
    {
        this.repository = repository;
    }

    public void Handle(Kit kit, ReserveKitCommand reserveKitCommand)
    {
        var kit = _repository.GetById(reserveKitCommand.KitId)
            ?? throw new Exception("Kit not found");

        var period = new DateRange(
            reserveKitCommand.StartDate,
            reserveKitCommand.EndDate);

        kit.Reserve(period);

        _repository.Save(kit);
    }
}
