using DecorRental.Domain.Enums;
using DecorRental.Domain.Repositories;
using DecorRental.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace DecorRental.Infrastructure.Repositories;

public sealed class EfReservationQueryRepository : IReservationQueryRepository
{
    private readonly DecorRentalDbContext _context;

    public EfReservationQueryRepository(DecorRentalDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ActiveReservationItem>> GetActiveReservationItemsAsync(
        DateOnly requestStartDate,
        DateOnly requestEndDate,
        IReadOnlyCollection<Guid> itemTypeIds,
        Guid? excludedReservationId = null,
        CancellationToken cancellationToken = default)
    {
        if (itemTypeIds.Count == 0)
        {
            return [];
        }

        var rows = await _context.Reservations
            .AsNoTracking()
            .Where(reservation => reservation.Status == ReservationStatus.Active)
            .Where(reservation => !excludedReservationId.HasValue || reservation.Id != excludedReservationId.Value)
            .Where(reservation => reservation.Period.Start < requestEndDate && reservation.Period.End > requestStartDate)
            .SelectMany(
                reservation => reservation.Items,
                (reservation, reservationItem) => new
                {
                    reservation.Id,
                    reservationItem.ItemTypeId,
                    reservationItem.Quantity,
                    StartDate = reservation.Period.Start,
                    EndDate = reservation.Period.End
                })
            .Where(row => itemTypeIds.Contains(row.ItemTypeId))
            .ToListAsync(cancellationToken);

        return rows
            .Select(row => new ActiveReservationItem(row.Id, row.ItemTypeId, row.Quantity, row.StartDate, row.EndDate))
            .ToList();
    }
}
