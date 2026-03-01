namespace DecorRental.Application.Contracts;

public interface IAddressLookupService
{
    Task<AddressLookupResult?> LookupByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default);
}
