using DecorRental.Application.Exceptions;
using DecorRental.Application.IntegrationEvents;
using DecorRental.Application.Messaging;
using DecorRental.Domain.Entities;
using DecorRental.Domain.Exceptions;
using DecorRental.Domain.Repositories;
using DecorRental.Domain.ValueObjects;

namespace DecorRental.Application.UseCases.UpdateReservation;

public sealed class UpdateReservationHandler
{
    private readonly IKitThemeRepository _kitThemeRepository;
    private readonly IKitCategoryRepository _categoryRepository;
    private readonly IItemTypeRepository _itemTypeRepository;
    private readonly IReservationQueryRepository _reservationQueryRepository;
    private readonly IMessageBus _messageBus;

    public UpdateReservationHandler(
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

    public async Task<UpdateReservationResult> HandleAsync(
        UpdateReservationCommand command,
        CancellationToken cancellationToken = default)
    {
        var kitTheme = await _kitThemeRepository.GetByIdAsync(command.KitThemeId, cancellationToken)
            ?? throw new NotFoundException("Tema de kit nao encontrado.");

        var existingReservation = kitTheme.Reservations.FirstOrDefault(
            reservation => reservation.Id == command.ReservationId);
        if (existingReservation is null)
        {
            throw new NotFoundException("Reserva nao encontrada.");
        }

        var category = await _categoryRepository.GetByIdAsync(command.KitCategoryId, cancellationToken)
            ?? throw new NotFoundException("Categoria nao encontrada.");

        var period = new DateRange(command.StartDate, command.EndDate);
        var stockShortages = await GetStockShortagesAsync(
            category,
            period,
            command.ReservationId,
            cancellationToken);

        if (stockShortages.Count > 0 && !command.AllowStockOverride)
        {
            var primaryShortage = stockShortages[0];
            throw new ConflictException(
                $"Estoque insuficiente para o item '{primaryShortage.ItemName}' no periodo selecionado.");
        }

        var isStockOverrideEffective = stockShortages.Count > 0 && command.AllowStockOverride;
        var reservation = kitTheme.UpdateReservation(
            command.ReservationId,
            category,
            period,
            isStockOverrideEffective,
            command.StockOverrideReason,
            command.CustomerName,
            command.CustomerDocumentNumber,
            command.CustomerPhoneNumber,
            command.CustomerAddress,
            command.Notes,
            command.HasBalloonArch,
            command.IsEntryPaid);

        await _kitThemeRepository.SaveAsync(kitTheme, cancellationToken);

        var integrationEvent = new ReservationUpdatedEvent(
            kitTheme.Id,
            reservation.KitCategoryId,
            reservation.Id,
            reservation.Period.Start,
            reservation.Period.End,
            reservation.Status.ToString());
        await _messageBus.PublishAsync(integrationEvent, cancellationToken);

        return new UpdateReservationResult(
            reservation.Id,
            kitTheme.Id,
            reservation.KitCategoryId,
            reservation.Period.Start,
            reservation.Period.End,
            reservation.Status.ToString(),
            reservation.IsStockOverride,
            reservation.StockOverrideReason,
            reservation.CustomerName,
            reservation.CustomerDocumentNumber,
            reservation.CustomerPhoneNumber,
            reservation.CustomerAddress,
            reservation.Notes,
            reservation.HasBalloonArch,
            reservation.IsEntryPaid);
    }

    private async Task<IReadOnlyList<StockShortage>> GetStockShortagesAsync(
        KitCategory category,
        DateRange period,
        Guid reservationIdToExclude,
        CancellationToken cancellationToken)
    {
        var requestedItemQuantities = category.Items
            .ToDictionary(item => item.ItemTypeId, item => item.Quantity);

        var itemTypeIds = requestedItemQuantities.Keys.ToArray();
        var itemTypes = await _itemTypeRepository.GetByIdsAsync(itemTypeIds, cancellationToken);
        if (itemTypes.Count != itemTypeIds.Length)
        {
            throw new DomainException("A categoria referencia tipos de item desconhecidos.");
        }

        var activeReservations = await _reservationQueryRepository.GetActiveReservationItemsAsync(
            period.Start,
            period.End,
            itemTypeIds,
            reservationIdToExclude,
            cancellationToken);

        var shortages = new List<StockShortage>();
        foreach (var itemType in itemTypes)
        {
            var requiredQuantity = requestedItemQuantities[itemType.Id];
            var maxReservedQuantity = CalculateMaxReservedQuantity(itemType.Id, period, activeReservations);
            var futureTotal = maxReservedQuantity + requiredQuantity;

            if (futureTotal > itemType.TotalStock)
            {
                shortages.Add(new StockShortage(itemType.Name));
            }
        }

        return shortages;
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

    private sealed record StockShortage(string ItemName);
}
