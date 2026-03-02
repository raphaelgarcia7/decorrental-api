using DecorRental.Api.Contracts;
using DecorRental.Api.Controllers;
using DecorRental.Application.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace DecorRental.Tests.Api;

public sealed class AddressControllerTests
{
    [Fact]
    public async Task LookupByZipCode_should_return_bad_request_when_zip_code_is_invalid()
    {
        var controller = CreateController(new StubAddressLookupService(null), "/api/address/lookup-cep/123");

        var actionResult = await controller.LookupByZipCode("123", CancellationToken.None);

        var badRequest = Assert.IsType<BadRequestObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(badRequest.Value);
        Assert.Equal(StatusCodes.Status400BadRequest, problemDetails.Status);
    }

    [Fact]
    public async Task LookupByZipCode_should_return_not_found_when_service_returns_null()
    {
        var controller = CreateController(new StubAddressLookupService(null), "/api/address/lookup-cep/01001000");

        var actionResult = await controller.LookupByZipCode("01001000", CancellationToken.None);

        var notFound = Assert.IsType<NotFoundObjectResult>(actionResult);
        var problemDetails = Assert.IsType<ProblemDetails>(notFound.Value);
        Assert.Equal(StatusCodes.Status404NotFound, problemDetails.Status);
    }

    [Fact]
    public async Task LookupByZipCode_should_return_ok_with_payload_when_service_finds_address()
    {
        var expectedAddress = new AddressLookupResult(
            ZipCode: "01001000",
            Street: "Praca da Se",
            Neighborhood: "Se",
            City: "Sao Paulo",
            State: "SP");
        var controller = CreateController(
            new StubAddressLookupService(expectedAddress),
            "/api/address/lookup-cep/01001000");

        var actionResult = await controller.LookupByZipCode("01001-000", CancellationToken.None);

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        var response = Assert.IsType<AddressLookupResponse>(okResult.Value);
        Assert.Equal(expectedAddress.ZipCode, response.ZipCode);
        Assert.Equal(expectedAddress.Street, response.Street);
        Assert.Equal(expectedAddress.Neighborhood, response.Neighborhood);
        Assert.Equal(expectedAddress.City, response.City);
        Assert.Equal(expectedAddress.State, response.State);
    }

    [Fact]
    public async Task LookupByZipCode_should_return_service_unavailable_when_lookup_service_throws_http_request_exception()
    {
        var controller = CreateController(new ThrowingAddressLookupService(), "/api/address/lookup-cep/01001000");

        var actionResult = await controller.LookupByZipCode("01001000", CancellationToken.None);

        var serviceUnavailable = Assert.IsType<ObjectResult>(actionResult);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, serviceUnavailable.StatusCode);
        var problemDetails = Assert.IsType<ProblemDetails>(serviceUnavailable.Value);
        Assert.Equal(StatusCodes.Status503ServiceUnavailable, problemDetails.Status);
    }

    private static AddressController CreateController(IAddressLookupService lookupService, string requestPath)
    {
        var controller = new AddressController(lookupService);
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Path = requestPath;

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }

    private sealed class StubAddressLookupService : IAddressLookupService
    {
        private readonly AddressLookupResult? _result;

        public StubAddressLookupService(AddressLookupResult? result)
        {
            _result = result;
        }

        public Task<AddressLookupResult?> LookupByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_result);
        }
    }

    private sealed class ThrowingAddressLookupService : IAddressLookupService
    {
        public Task<AddressLookupResult?> LookupByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
        {
            throw new HttpRequestException("ViaCEP unavailable.");
        }
    }
}
