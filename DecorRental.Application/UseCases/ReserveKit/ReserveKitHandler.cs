using DecorRental.Domain.Entities;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Application.UseCases.ReserveKit;

public class ReserveKitHandler
{
    public void Handle(Kit kit, ReserveKitCommand reserveKitCommand)
    {
        var period = new DateRange(
            reserveKitCommand.StartDate,
            reserveKitCommand.EndDate);

        kit.Reserve(period);
    }
}
