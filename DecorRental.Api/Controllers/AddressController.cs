using DecorRental.Api.Contracts;
using DecorRental.Api.Middleware;
using DecorRental.Api.Security;
using DecorRental.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DecorRental.Api.Controllers;

[ApiController]
[Route("api/address")]
[Authorize(Policy = AuthorizationPolicies.ReadKits)]
public sealed class AddressController : ControllerBase
{
    private readonly IAddressLookupService _addressLookupService;

    public AddressController(IAddressLookupService addressLookupService)
    {
        _addressLookupService = addressLookupService;
    }

    [HttpGet("lookup-cep/{zipCode}")]
    [ProducesResponseType(typeof(AddressLookupResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LookupByZipCode(string zipCode, CancellationToken cancellationToken)
    {
        var normalizedZipCode = new string(zipCode.Where(char.IsDigit).ToArray());
        if (normalizedZipCode.Length != 8)
        {
            return BadRequest(BuildProblemDetails(
                StatusCodes.Status400BadRequest,
                "https://httpstatuses.com/400",
                "Falha de validacao.",
                "Informe um CEP com 8 digitos."));
        }

        AddressLookupResult? result;
        try
        {
            result = await _addressLookupService.LookupByZipCodeAsync(normalizedZipCode, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                BuildProblemDetails(
                    StatusCodes.Status503ServiceUnavailable,
                    "https://httpstatuses.com/503",
                    "Servico de CEP indisponivel.",
                    "Nao foi possivel consultar o CEP no momento. Tente novamente em instantes."));
        }
        catch (TaskCanceledException)
        {
            return StatusCode(
                StatusCodes.Status503ServiceUnavailable,
                BuildProblemDetails(
                    StatusCodes.Status503ServiceUnavailable,
                    "https://httpstatuses.com/503",
                    "Servico de CEP indisponivel.",
                    "A consulta de CEP excedeu o tempo limite. Tente novamente em instantes."));
        }

        if (result is null)
        {
            return NotFound(BuildProblemDetails(
                StatusCodes.Status404NotFound,
                "https://httpstatuses.com/404",
                "CEP nao encontrado.",
                "Nao foi possivel localizar o CEP informado."));
        }

        var response = new AddressLookupResponse(
            result.ZipCode,
            result.Street,
            result.Neighborhood,
            result.City,
            result.State);

        return Ok(response);
    }

    private ProblemDetails BuildProblemDetails(int statusCode, string type, string title, string detail)
    {
        var problemDetails = new ProblemDetails
        {
            Type = type,
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = HttpContext.Request.Path
        };

        var correlationId = CorrelationIdMiddleware.ResolveCorrelationId(HttpContext);
        problemDetails.Extensions["traceId"] = HttpContext.TraceIdentifier;
        problemDetails.Extensions["correlationId"] = correlationId;
        return problemDetails;
    }
}
