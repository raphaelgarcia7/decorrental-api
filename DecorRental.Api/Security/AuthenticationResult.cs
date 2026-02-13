namespace DecorRental.Api.Security;

public sealed record AuthenticationResult(
    bool IsAuthenticated,
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    string Role)
{
    public static AuthenticationResult Failed()
        => new(false, string.Empty, DateTimeOffset.MinValue, string.Empty);
}
