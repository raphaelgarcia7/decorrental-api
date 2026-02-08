using DecorRental.Domain.Entities;
using DecorRental.Domain.Repositories;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Application.UseCases.ReserveKit;

public class ReserveKitHandler
{

    private readonly IKitRepository _repository;

    public ReserveKitHandler(IKitRepository repository)
    {
        _repository = repository;
    }

    public void Handle(ReserveKitCommand reserveKitCommand)
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
