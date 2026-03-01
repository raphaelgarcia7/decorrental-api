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

        var (legacyAddressLine, legacyNeighborhood, legacyCity) = SplitAddress(reservation.CustomerAddress);
        var addressLine = BuildAddressLine(
            reservation.CustomerStreet,
            reservation.CustomerNumber,
            reservation.CustomerComplement,
            legacyAddressLine);
        var neighborhood = reservation.CustomerNeighborhood ?? legacyNeighborhood;
        var city = reservation.CustomerCity ?? legacyCity;

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
            addressLine,
            reservation.CustomerZipCode,
            reservation.CustomerStreet,
            reservation.CustomerNumber,
            reservation.CustomerComplement,
            reservation.CustomerState,
            reservation.CustomerReference,
            neighborhood,
            city,
            reservation.Notes,
            reservation.HasBalloonArch,
            reservation.IsEntryPaid,
            DateOnly.FromDateTime(DateTime.Today),
            null,
            null);
    }

    private static string BuildAddressLine(
        string? street,
        string? number,
        string? complement,
        string fallbackAddressLine)
    {
        if (string.IsNullOrWhiteSpace(street) || string.IsNullOrWhiteSpace(number))
        {
            return fallbackAddressLine;
        }

        var addressLine = $"{street.Trim()}, {number.Trim()}";
        if (!string.IsNullOrWhiteSpace(complement))
        {
            addressLine = $"{addressLine} - {complement.Trim()}";
        }

        return addressLine;
    }

    private static (string AddressLine, string? Neighborhood, string? City) SplitAddress(string address)
    {
        if (string.IsNullOrWhiteSpace(address))
        {
            return (string.Empty, null, null);
        }

        var parts = address
            .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

        return parts.Length switch
        {
            >= 3 => (string.Join(", ", parts[..^2]), parts[^2], parts[^1]),
            2 => (parts[0], parts[1], null),
            _ => (parts[0], null, null)
        };
    }
}
