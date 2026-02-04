using DecorRental.Domain.Entities;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Application.UseCases.ReserveKit;

public class ReserveKitHandler
{
    public void Handle(Kit kit, ReserveKitCommand command)
    {
        var period = new DateRange(
            command.StartDate,
            command.EndDate);

        kit.Reserve(period);
    }
}
