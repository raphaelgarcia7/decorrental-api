namespace DecorRental.Application.Contracts;

public sealed record AddressLookupResult(
    string ZipCode,
    string Street,
    string Neighborhood,
    string City,
    string State);
