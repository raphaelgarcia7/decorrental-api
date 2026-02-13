using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DecorRental.Api.Security;

public sealed class JwtAuthenticationService : IAuthenticationService
{
    private readonly JwtOptions _jwtOptions;
    private readonly SigningCredentials _signingCredentials;

    public JwtAuthenticationService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;
        ValidateOptions(_jwtOptions);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SigningKey));
        _signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
    }

    public AuthenticationResult Authenticate(string username, string password)
    {
        var matchedUser = _jwtOptions.Users.FirstOrDefault(user =>
            string.Equals(user.Username, username, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(user.Password, password, StringComparison.Ordinal));

        if (matchedUser is null)
        {
            return AuthenticationResult.Failed();
        }

        var expiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(_jwtOptions.TokenExpirationMinutes);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, matchedUser.Username),
            new(JwtRegisteredClaimNames.UniqueName, matchedUser.Username),
            new(ClaimTypes.Name, matchedUser.Username),
            new(ClaimTypes.Role, matchedUser.Role)
        };

        var tokenDescriptor = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            expires: expiresAtUtc.UtcDateTime,
            signingCredentials: _signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        return new AuthenticationResult(true, accessToken, expiresAtUtc, matchedUser.Role);
    }

    private static void ValidateOptions(JwtOptions jwtOptions)
    {
        if (string.IsNullOrWhiteSpace(jwtOptions.Issuer) ||
            string.IsNullOrWhiteSpace(jwtOptions.Audience) ||
            string.IsNullOrWhiteSpace(jwtOptions.SigningKey))
        {
            throw new InvalidOperationException(
                "JWT configuration is invalid. Set Issuer, Audience and SigningKey in configuration.");
        }

        if (jwtOptions.SigningKey.Length < 32)
        {
            throw new InvalidOperationException("JWT signing key must have at least 32 characters.");
        }
    }
}
