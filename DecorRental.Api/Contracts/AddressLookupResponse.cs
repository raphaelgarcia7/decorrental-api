namespace DecorRental.Api.Contracts;

public sealed record AddressLookupResponse(
    string ZipCode,
    string Street,
    string Neighborhood,
    string City,
    string State);
