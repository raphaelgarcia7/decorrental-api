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
                throw new DomainException("A data de termino deve ser posterior a data de inicio.");

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
