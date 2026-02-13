namespace DecorRental.Api.Contracts;

public sealed record AuthTokenResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string TokenType,
    string Role);
