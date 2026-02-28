namespace DecorRental.Domain.Repositories;

public sealed record ActiveReservationItem(
    Guid ReservationId,
    Guid ItemTypeId,
    int Quantity,
    DateOnly StartDate,
    DateOnly EndDate);

public interface IReservationQueryRepository
{
    Task<IReadOnlyList<ActiveReservationItem>> GetActiveReservationItemsAsync(
        DateOnly requestStartDate,
        DateOnly requestEndDate,
        IReadOnlyCollection<Guid> itemTypeIds,
        Guid? excludedReservationId = null,
        CancellationToken cancellationToken = default);
}
