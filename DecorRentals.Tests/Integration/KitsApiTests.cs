using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace DecorRental.Tests.Integration;

public sealed class KitsApiTests : IClassFixture<DecorRentalApiFactory>
{
    private readonly HttpClient _httpClient;

    public KitsApiTests(DecorRentalApiFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Reserve_endpoint_should_create_reservation_when_period_is_available()
    {
        var kitId = await CreateKitAsync("Integration Kit");

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest("2026-03-10", "2026-03-12"));

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        var reservationsResponse = await _httpClient.GetAsync($"/api/kits/{kitId}/reservations");
        var reservations = await reservationsResponse.Content.ReadFromJsonAsync<List<ReservationResponse>>();

        Assert.Equal(HttpStatusCode.OK, reservationsResponse.StatusCode);
        Assert.NotNull(reservations);
        Assert.Single(reservations);
    }

    [Fact]
    public async Task Reserve_endpoint_should_return_conflict_when_period_overlaps()
    {
        var kitId = await CreateKitAsync("Conflict Kit");

        await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest("2026-04-10", "2026-04-12"));

        var conflictResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest("2026-04-11", "2026-04-13"));

        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);

        var error = await conflictResponse.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal("conflict", error.Code);
    }

    [Fact]
    public async Task Cancel_endpoint_should_free_period_for_new_reservation()
    {
        var kitId = await CreateKitAsync("Cancel Kit");

        await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest("2026-05-01", "2026-05-03"));

        var reservationsResponse = await _httpClient.GetAsync($"/api/kits/{kitId}/reservations");
        var reservations = await reservationsResponse.Content.ReadFromJsonAsync<List<ReservationResponse>>();

        Assert.NotNull(reservations);
        var reservationId = reservations[0].Id;

        var cancelResponse = await _httpClient.PostAsync(
            $"/api/kits/{kitId}/reservations/{reservationId}/cancel",
            content: null);

        Assert.Equal(HttpStatusCode.NoContent, cancelResponse.StatusCode);

        var reserveAgainResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest("2026-05-01", "2026-05-03"));

        Assert.Equal(HttpStatusCode.NoContent, reserveAgainResponse.StatusCode);
    }

    private async Task<Guid> CreateKitAsync(string name)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/kits", new CreateKitRequest(name));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<KitSummaryResponse>();
        if (body is null)
        {
            throw new InvalidOperationException("Create kit response body is null.");
        }

        return body.Id;
    }

    private sealed record CreateKitRequest(string Name);

    private sealed record ReserveKitRequest(string StartDate, string EndDate);

    private sealed record KitSummaryResponse(Guid Id, string Name);

    private sealed record ReservationResponse(Guid Id, string StartDate, string EndDate, string Status);

    private sealed record ErrorResponse(string Code, string Message);
}
