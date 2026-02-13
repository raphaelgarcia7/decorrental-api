namespace DecorRental.Api.Security;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;

    public string Audience { get; init; } = string.Empty;

    public string SigningKey { get; init; } = string.Empty;

    public int TokenExpirationMinutes { get; init; } = 60;

    public IReadOnlyList<JwtUserOptions> Users { get; init; } = [];
}

public sealed class JwtUserOptions
{
    public string Username { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string Role { get; init; } = string.Empty;
}
