using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
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
    public async Task Create_endpoint_should_return_unauthorized_when_token_is_missing()
    {
        var response = await _httpClient.PostAsJsonAsync("/api/kits", new CreateKitRequest("No Token"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("unauthorized", await ReadProblemCodeAsync(response));
    }

    [Fact]
    public async Task Create_endpoint_should_return_forbidden_for_viewer_role()
    {
        await AuthenticateAsAsync("viewer", "viewer123");

        var response = await _httpClient.PostAsJsonAsync("/api/kits", new CreateKitRequest("Viewer Kit"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("forbidden", await ReadProblemCodeAsync(response));
    }

    [Fact]
    public async Task Reserve_endpoint_should_create_reservation_when_period_is_available()
    {
        await AuthenticateAsManagerAsync();
        var kitId = await CreateKitAsync("Integration Kit");

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest("2026-03-10", "2026-03-12"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var reserveResponse = await response.Content.ReadFromJsonAsync<ReserveKitResponse>();
        Assert.NotNull(reserveResponse);
        Assert.Equal(kitId, reserveResponse.KitId);
        Assert.Equal("Active", reserveResponse.Status);

        var reservationsResponse = await _httpClient.GetAsync($"/api/kits/{kitId}/reservations");
        var reservations = await reservationsResponse.Content.ReadFromJsonAsync<List<ReservationResponse>>();

        Assert.Equal(HttpStatusCode.OK, reservationsResponse.StatusCode);
        Assert.NotNull(reservations);
        Assert.Single(reservations);
    }

    [Fact]
    public async Task Reserve_endpoint_should_return_conflict_when_period_overlaps()
    {
        await AuthenticateAsManagerAsync();
        var kitId = await CreateKitAsync("Conflict Kit");

        await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest("2026-04-10", "2026-04-12"));

        var conflictResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest("2026-04-11", "2026-04-13"));

        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
        Assert.Equal("conflict", await ReadProblemCodeAsync(conflictResponse));
    }

    [Fact]
    public async Task Reserve_endpoint_should_return_bad_request_when_end_date_is_before_start_date()
    {
        await AuthenticateAsManagerAsync();
        var kitId = await CreateKitAsync("Validation Kit");

        var invalidResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest("2026-06-10", "2026-06-09"));

        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
        Assert.Equal("validation_error", await ReadProblemCodeAsync(invalidResponse));
        Assert.True(await ReadValidationErrorsCountAsync(invalidResponse) > 0);
    }

    [Fact]
    public async Task Get_kits_endpoint_should_return_paginated_result()
    {
        await AuthenticateAsManagerAsync();
        await CreateKitAsync("Paged Kit A");
        await CreateKitAsync("Paged Kit B");
        await CreateKitAsync("Paged Kit C");

        var response = await _httpClient.GetAsync("/api/kits?page=1&pageSize=2");
        var pagedResponse = await response.Content.ReadFromJsonAsync<PagedResponse<KitSummaryResponse>>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(pagedResponse);
        Assert.Equal(1, pagedResponse.Page);
        Assert.Equal(2, pagedResponse.PageSize);
        Assert.Equal(2, pagedResponse.Items.Count);
        Assert.True(pagedResponse.TotalCount >= 3);
    }

    [Fact]
    public async Task Get_kits_endpoint_should_return_bad_request_when_page_size_is_invalid()
    {
        await AuthenticateAsManagerAsync();
        var response = await _httpClient.GetAsync("/api/kits?page=1&pageSize=101");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("validation_error", await ReadProblemCodeAsync(response));
    }

    [Fact]
    public async Task Cancel_endpoint_should_free_period_for_new_reservation()
    {
        await AuthenticateAsManagerAsync();
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

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);
        var cancelReservationResponse = await cancelResponse.Content.ReadFromJsonAsync<CancelReservationResponse>();
        Assert.NotNull(cancelReservationResponse);
        Assert.Equal(reservationId, cancelReservationResponse.ReservationId);
        Assert.Equal("Cancelled", cancelReservationResponse.Status);

        var reserveAgainResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest("2026-05-01", "2026-05-03"));

        Assert.Equal(HttpStatusCode.OK, reserveAgainResponse.StatusCode);
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

    private Task AuthenticateAsManagerAsync()
        => AuthenticateAsAsync("manager", "manager123");

    private async Task AuthenticateAsAsync(string username, string password)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/auth/token", new AuthTokenRequest(username, password));
        response.EnsureSuccessStatusCode();

        var authResponse = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        if (authResponse is null)
        {
            throw new InvalidOperationException("Authentication response body is null.");
        }

        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", authResponse.AccessToken);
    }

    private sealed record CreateKitRequest(string Name);

    private sealed record ReserveKitRequest(string StartDate, string EndDate);

    private sealed record AuthTokenRequest(string Username, string Password);

    private sealed record AuthTokenResponse(string AccessToken);

    private sealed record KitSummaryResponse(Guid Id, string Name);

    private sealed record ReservationResponse(Guid Id, string StartDate, string EndDate, string Status);

    private sealed record ReserveKitResponse(
        Guid ReservationId,
        Guid KitId,
        string StartDate,
        string EndDate,
        string Status,
        string Message);

    private sealed record CancelReservationResponse(
        Guid ReservationId,
        Guid KitId,
        string Status,
        string Message);

    private sealed record PagedResponse<TItem>(
        IReadOnlyList<TItem> Items,
        int Page,
        int PageSize,
        int TotalCount);

    private static async Task<string?> ReadProblemCodeAsync(HttpResponseMessage response)
    {
        using var document = await ReadJsonDocumentAsync(response);
        var root = document.RootElement;

        if (root.TryGetProperty("code", out var codeProperty) && codeProperty.ValueKind == JsonValueKind.String)
        {
            return codeProperty.GetString();
        }

        if (root.TryGetProperty("extensions", out var extensionsProperty) &&
            extensionsProperty.ValueKind == JsonValueKind.Object &&
            extensionsProperty.TryGetProperty("code", out var nestedCodeProperty) &&
            nestedCodeProperty.ValueKind == JsonValueKind.String)
        {
            return nestedCodeProperty.GetString();
        }

        return null;
    }

    private static async Task<int> ReadValidationErrorsCountAsync(HttpResponseMessage response)
    {
        using var document = await ReadJsonDocumentAsync(response);
        var root = document.RootElement;

        if (root.TryGetProperty("errors", out var errorsProperty) && errorsProperty.ValueKind == JsonValueKind.Object)
        {
            return errorsProperty.EnumerateObject().Count();
        }

        if (root.TryGetProperty("extensions", out var extensionsProperty) &&
            extensionsProperty.ValueKind == JsonValueKind.Object &&
            extensionsProperty.TryGetProperty("errors", out var nestedErrorsProperty) &&
            nestedErrorsProperty.ValueKind == JsonValueKind.Object)
        {
            return nestedErrorsProperty.EnumerateObject().Count();
        }

        return 0;
    }

    private static async Task<JsonDocument> ReadJsonDocumentAsync(HttpResponseMessage response)
    {
        var payload = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(payload))
        {
            throw new InvalidOperationException("Response payload is empty.");
        }

        return JsonDocument.Parse(payload);
    }
}
