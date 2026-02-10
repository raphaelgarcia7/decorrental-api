using DecorRental.Domain.Enums;
using DecorRental.Domain.Exceptions;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Domain.Entities;

public class Kit
{
    public Guid Id { get; private set; }
    public string Name { get; private set; }

    public IReadOnlyCollection<Reservation> Reservations => _reservations;

    public Kit(string name)
    {
        Id = Guid.NewGuid();
        Name = name;
    }

    public void Reserve(DateRange period)
    {
        var hasConflict = _reservations.Any(r =>
            r.Status == ReservationStatus.Active &&
            r.Period.Overlaps(period));

        if (hasConflict)
            throw new DomainException("Kit is already reserved for this period.");

        _reservations.Add(new Reservation(Id, period));
    }

    private readonly List<Reservation> _reservations = new();

    private Kit() { }
}
