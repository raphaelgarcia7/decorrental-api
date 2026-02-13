using DecorRental.Api.Contracts;
using DecorRental.Api.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorRental.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService;

    public AuthController(IAuthenticationService authenticationService)
    {
        _authenticationService = authenticationService;
    }

    [AllowAnonymous]
    [HttpPost("token")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public IActionResult GenerateToken([FromBody] AuthTokenRequest request)
    {
        var authenticationResult = _authenticationService.Authenticate(request.Username, request.Password);
        if (!authenticationResult.IsAuthenticated)
        {
            return Unauthorized();
        }

        var response = new AuthTokenResponse(
            authenticationResult.AccessToken,
            authenticationResult.ExpiresAtUtc,
            "Bearer",
            authenticationResult.Role);

        return Ok(response);
    }
}
