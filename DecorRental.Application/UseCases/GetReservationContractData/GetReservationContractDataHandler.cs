using DecorRental.Application.Contracts;
using DecorRental.Application.Exceptions;
using DecorRental.Domain.Repositories;

namespace DecorRental.Application.UseCases.GetReservationContractData;

public sealed class GetReservationContractDataHandler
{
    private readonly IKitThemeRepository _kitThemeRepository;
    private readonly IKitCategoryRepository _kitCategoryRepository;

    public GetReservationContractDataHandler(
        IKitThemeRepository kitThemeRepository,
        IKitCategoryRepository kitCategoryRepository)
    {
        _kitThemeRepository = kitThemeRepository;
        _kitCategoryRepository = kitCategoryRepository;
    }

    public async Task<ContractData> HandleAsync(
        GetReservationContractDataQuery query,
        CancellationToken cancellationToken = default)
    {
        var kitTheme = await _kitThemeRepository.GetByIdAsync(query.KitThemeId, cancellationToken)
            ?? throw new NotFoundException("Tema de kit nao encontrado.");

        var reservation = kitTheme.Reservations.FirstOrDefault(currentReservation => currentReservation.Id == query.ReservationId)
            ?? throw new NotFoundException("Reserva nao encontrada para este tema de kit.");

        var category = await _kitCategoryRepository.GetByIdAsync(reservation.KitCategoryId, cancellationToken)
            ?? throw new NotFoundException("Categoria nao encontrada.");

        return new ContractData(
            kitTheme.Id,
            reservation.Id,
            kitTheme.Name,
            category.Name,
            reservation.Period.Start,
            reservation.Period.End,
            reservation.CustomerName,
            reservation.CustomerDocumentNumber,
            reservation.CustomerPhoneNumber,
            reservation.CustomerAddress,
            reservation.Notes,
            reservation.HasBalloonArch,
            reservation.IsEntryPaid,
            DateOnly.FromDateTime(DateTime.Today));
    }
}
