using DecorRental.Domain.Exceptions;

namespace DecorRental.Domain.ValueObjects
{
    public sealed class DateRange
    {
        public DateOnly Start { get; private set; }
        public DateOnly End { get; private set; }

        public DateRange(DateOnly start, DateOnly end)
        {
            if (end < start)
                throw new DomainException("End date must be after start date.");

            Start = start;
            End = end;
        }

        public bool Overlaps(DateRange other)
        {
            return Start <= other.End && End >= other.Start;
        }

        private DateRange() { }
    }
}
