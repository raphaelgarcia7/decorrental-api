namespace DecorRental.Domain.Repositories;

public sealed record ActiveReservationItem(
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
        CancellationToken cancellationToken = default);
}
