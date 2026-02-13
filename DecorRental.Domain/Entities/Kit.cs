using DecorRental.Domain.Enums;
using DecorRental.Domain.Exceptions;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Domain.Entities;

public class Kit
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    public IReadOnlyCollection<Reservation> Reservations => _reservations;

    public Kit(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Kit name is required.");

        Id = Guid.NewGuid();
        Name = name;
    }

    public void Reserve(DateRange period)
    {
        var hasConflict = _reservations.Any(reservation =>
            reservation.Status == ReservationStatus.Active &&
            reservation.Period.Overlaps(period));

        if (hasConflict)
            throw new ConflictException("Kit is already reserved for this period.");

        _reservations.Add(new Reservation(Id, period));
    }

    public void CancelReservation(Guid reservationId)
    {
        var reservation = _reservations.FirstOrDefault(currentReservation => currentReservation.Id == reservationId);
        if (reservation is null)
            throw new DomainException("Reservation not found for this kit.");

        reservation.Cancel();
    }

    private readonly List<Reservation> _reservations = new();

    private Kit() { }
}
