using DecorRental.Domain.Enums;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Domain.Entities
{
    public class Reservation
    {
        public Guid Id { get; private set; }
        public Guid KitId { get; private set; }
        public DateRange Period { get; private set; }
        public ReservationStatus Status { get; private set; }

        public Reservation(Guid kitId, DateRange period)
        {
            Id = Guid.NewGuid();
            KitId = kitId;
            Period = period;
            Status = ReservationStatus.Active;
        }

        public void Cancel()
        {
            Status = ReservationStatus.Cancelled;
        }

        private Reservation() { }
    }
}
