using DecorRental.Domain.Enums;
using DecorRental.Domain.Exceptions;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Domain.Entities;

public class Reservation
{
    private readonly List<ReservationItem> _items = new();

    public Guid Id { get; private set; }
    public Guid KitThemeId { get; private set; }
    public Guid KitCategoryId { get; private set; }
    public DateRange Period { get; private set; } = null!;
    public ReservationStatus Status { get; private set; }
    public IReadOnlyCollection<ReservationItem> Items => _items;

    public static Reservation Create(Guid kitThemeId, KitCategory category, DateRange period)
    {
        if (category is null)
        {
            throw new DomainException("Category is required.");
        }

        if (category.Items.Count == 0)
        {
            throw new DomainException("Category must have at least one item.");
        }

        var reservationId = Guid.NewGuid();
        var reservationItems = category.Items
            .Select(categoryItem => new ReservationItem(reservationId, categoryItem.ItemTypeId, categoryItem.Quantity))
            .ToList();

        return new Reservation(reservationId, kitThemeId, category.Id, period, reservationItems);
    }

    private Reservation(
        Guid reservationId,
        Guid kitThemeId,
        Guid kitCategoryId,
        DateRange period,
        IReadOnlyCollection<ReservationItem> items)
    {
        if (items.Count == 0)
        {
            throw new DomainException("Reservation must contain at least one item.");
        }

        Id = reservationId;
        KitThemeId = kitThemeId;
        KitCategoryId = kitCategoryId;
        Period = period;
        Status = ReservationStatus.Active;

        _items.AddRange(items);
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
        {
            return;
        }

        Status = ReservationStatus.Cancelled;
    }

    private Reservation() { }
}
