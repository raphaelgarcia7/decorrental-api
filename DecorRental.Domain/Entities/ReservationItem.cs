using DecorRental.Domain.Exceptions;

namespace DecorRental.Domain.Entities;

public class ReservationItem
{
    public Guid Id { get; private set; }
    public Guid ReservationId { get; private set; }
    public Guid ItemTypeId { get; private set; }
    public int Quantity { get; private set; }

    public ReservationItem(Guid reservationId, Guid itemTypeId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new DomainException("A quantidade do item da reserva deve ser maior que zero.");
        }

        Id = Guid.NewGuid();
        ReservationId = reservationId;
        ItemTypeId = itemTypeId;
        Quantity = quantity;
    }

    private ReservationItem() { }
}
