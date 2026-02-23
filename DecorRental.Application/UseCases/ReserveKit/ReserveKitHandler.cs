using DecorRental.Application.Exceptions;
using DecorRental.Application.IntegrationEvents;
using DecorRental.Application.Messaging;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Exceptions;
using DecorRental.Domain.Repositories;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Application.UseCases.ReserveKit;

public sealed class ReserveKitHandler
{
    private readonly IKitThemeRepository _kitThemeRepository;
    private readonly IKitCategoryRepository _categoryRepository;
    private readonly IItemTypeRepository _itemTypeRepository;
    private readonly IReservationQueryRepository _reservationQueryRepository;
    private readonly IMessageBus _messageBus;

    public ReserveKitHandler(
        IKitThemeRepository kitThemeRepository,
        IKitCategoryRepository categoryRepository,
        IItemTypeRepository itemTypeRepository,
        IReservationQueryRepository reservationQueryRepository,
        IMessageBus messageBus)
    {
        _kitThemeRepository = kitThemeRepository;
        _categoryRepository = categoryRepository;
        _itemTypeRepository = itemTypeRepository;
        _reservationQueryRepository = reservationQueryRepository;
        _messageBus = messageBus;
    }

    public async Task<ReserveKitResult> HandleAsync(
        ReserveKitCommand command,
        CancellationToken cancellationToken = default)
    {
        var kitTheme = await _kitThemeRepository.GetByIdAsync(command.KitThemeId, cancellationToken)
            ?? throw new NotFoundException("Kit theme not found.");

        var category = await _categoryRepository.GetByIdAsync(command.KitCategoryId, cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        var period = new DateRange(command.StartDate, command.EndDate);
        await EnsureStockAvailabilityAsync(category, period, cancellationToken);

        var reservation = kitTheme.Reserve(category, period);

        await _kitThemeRepository.SaveAsync(kitTheme, cancellationToken);

        var integrationEvent = new ReservationCreatedEvent(
            kitTheme.Id,
            category.Id,
            reservation.Id,
            reservation.Period.Start,
            reservation.Period.End,
            reservation.Status.ToString());

        await _messageBus.PublishAsync(integrationEvent, cancellationToken);

        return new ReserveKitResult(
            reservation.Id,
            kitTheme.Id,
            category.Id,
            reservation.Period.Start,
            reservation.Period.End,
            reservation.Status.ToString());
    }

    private async Task EnsureStockAvailabilityAsync(
        KitCategory category,
        DateRange period,
        CancellationToken cancellationToken)
    {
        var requestedItemQuantities = category.Items
            .ToDictionary(item => item.ItemTypeId, item => item.Quantity);

        var itemTypeIds = requestedItemQuantities.Keys.ToArray();
        var itemTypes = await _itemTypeRepository.GetByIdsAsync(itemTypeIds, cancellationToken);
        if (itemTypes.Count != itemTypeIds.Length)
        {
            throw new DomainException("Category references unknown item types.");
        }

        var activeReservations = await _reservationQueryRepository.GetActiveReservationItemsAsync(
            period.Start,
            period.End,
            itemTypeIds,
            cancellationToken);

        foreach (var itemType in itemTypes)
        {
            var requiredQuantity = requestedItemQuantities[itemType.Id];
            var maxReservedQuantity = CalculateMaxReservedQuantity(itemType.Id, period, activeReservations);
            var futureTotal = maxReservedQuantity + requiredQuantity;

            if (futureTotal > itemType.TotalStock)
            {
                throw new ConflictException(
                    $"Insufficient stock for item '{itemType.Name}' in the selected period.");
            }
        }
    }

    private static int CalculateMaxReservedQuantity(
        Guid itemTypeId,
        DateRange requestPeriod,
        IReadOnlyList<ActiveReservationItem> activeReservations)
    {
        var events = new List<(DateOnly Date, int Delta)>();

        foreach (var reservationItem in activeReservations.Where(item => item.ItemTypeId == itemTypeId))
        {
            var overlapStart = reservationItem.StartDate > requestPeriod.Start
                ? reservationItem.StartDate
                : requestPeriod.Start;

            var overlapEnd = reservationItem.EndDate < requestPeriod.End
                ? reservationItem.EndDate
                : requestPeriod.End;

            if (overlapEnd < overlapStart)
            {
                continue;
            }

            events.Add((overlapStart, reservationItem.Quantity));
            events.Add((overlapEnd.AddDays(1), -reservationItem.Quantity));
        }

        var orderedEvents = events
            .OrderBy(item => item.Date)
            .ThenBy(item => item.Delta)
            .ToList();

        var runningTotal = 0;
        var maxReserved = 0;

        foreach (var currentEvent in orderedEvents)
        {
            runningTotal += currentEvent.Delta;
            if (runningTotal > maxReserved)
            {
                maxReserved = runningTotal;
            }
        }

        return maxReserved;
    }
}
