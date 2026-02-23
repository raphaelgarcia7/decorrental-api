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
            throw new DomainException("Reservation item quantity must be greater than zero.");
        }

        Id = Guid.NewGuid();
        ReservationId = reservationId;
        ItemTypeId = itemTypeId;
        Quantity = quantity;
    }

    private ReservationItem() { }
}
