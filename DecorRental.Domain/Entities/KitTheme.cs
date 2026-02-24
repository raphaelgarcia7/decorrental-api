using DecorRental.Domain.Exceptions;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Domain.Entities;

public class KitTheme
{
    private readonly List<Reservation> _reservations = new();

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public IReadOnlyCollection<Reservation> Reservations => _reservations;

    public KitTheme(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("O nome do tema do kit e obrigatorio.");
        }

        Id = Guid.NewGuid();
        Name = name.Trim();
    }

    public Reservation Reserve(
        KitCategory category,
        DateRange period,
        bool isStockOverride,
        string? stockOverrideReason)
    {
        if (category is null)
        {
            throw new DomainException("Categoria e obrigatoria.");
        }

        if (category.Items.Count == 0)
        {
            throw new DomainException("Category must have at least one item.");
        }

        var reservation = Reservation.Create(
            Id,
            category,
            period,
            isStockOverride,
            stockOverrideReason);
        _reservations.Add(reservation);

        return reservation;
    }

    public Reservation CancelReservation(Guid reservationId)
    {
        var reservation = _reservations.FirstOrDefault(currentReservation => currentReservation.Id == reservationId);
        if (reservation is null)
        {
            throw new DomainException("Reserva nao encontrada para este tema de kit.");
        }

        reservation.Cancel();
        return reservation;
    }

    private KitTheme() { }
}
