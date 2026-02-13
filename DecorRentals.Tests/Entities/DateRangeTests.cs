using DecorRental.Domain.Exceptions;
using DecorRental.Domain.ValueObjects;
using Xunit;

namespace DecorRental.Tests.Entities;

public class DateRangeTests
{
    [Fact]
    public void Should_throw_when_end_date_is_before_start_date()
    {
        Assert.Throws<DomainException>(() =>
            new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 9)));
    }

    [Fact]
    public void Should_detect_overlap_when_ranges_intersect()
    {
        var firstRange = new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 12));
        var secondRange = new DateRange(new DateOnly(2026, 1, 12), new DateOnly(2026, 1, 15));

        Assert.True(firstRange.Overlaps(secondRange));
    }

    [Fact]
    public void Should_not_overlap_when_ranges_are_apart()
    {
        var firstRange = new DateRange(new DateOnly(2026, 1, 10), new DateOnly(2026, 1, 12));
        var secondRange = new DateRange(new DateOnly(2026, 1, 13), new DateOnly(2026, 1, 15));

        Assert.False(firstRange.Overlaps(secondRange));
    }
}
