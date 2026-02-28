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
    public async Task Item_types_endpoints_should_return_complete_payloads()
    {
        await AuthenticateAsManagerAsync();

        var createResponse = await _httpClient.PostAsJsonAsync("/api/item-types", new CreateItemTypeRequest("Cylinder", 12));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        Assert.NotNull(createResponse.Headers.Location);

        var createdItem = await createResponse.Content.ReadFromJsonAsync<ItemTypeResponse>();
        Assert.NotNull(createdItem);
        Assert.Equal("Cylinder", createdItem.Name);
        Assert.Equal(12, createdItem.TotalStock);

        var getByIdResponse = await _httpClient.GetAsync($"/api/item-types/{createdItem.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
        var fetchedItem = await getByIdResponse.Content.ReadFromJsonAsync<ItemTypeResponse>();
        Assert.NotNull(fetchedItem);
        Assert.Equal(createdItem.Id, fetchedItem.Id);
        Assert.Equal(createdItem.Name, fetchedItem.Name);
        Assert.Equal(createdItem.TotalStock, fetchedItem.TotalStock);

        var updateResponse = await _httpClient.PatchAsJsonAsync($"/api/item-types/{createdItem.Id}/stock", new UpdateItemStockRequest(20));
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);
        var updatedItem = await updateResponse.Content.ReadFromJsonAsync<ItemTypeResponse>();
        Assert.NotNull(updatedItem);
        Assert.Equal(createdItem.Id, updatedItem.Id);
        Assert.Equal("Cylinder", updatedItem.Name);
        Assert.Equal(20, updatedItem.TotalStock);
    }

    [Fact]
    public async Task Categories_endpoints_should_return_complete_payloads()
    {
        await AuthenticateAsManagerAsync();

        var itemTypeName = $"Panel-{Guid.NewGuid():N}".Substring(0, 14);
        var createItemTypeResponse = await _httpClient.PostAsJsonAsync("/api/item-types", new CreateItemTypeRequest(itemTypeName, 8));
        createItemTypeResponse.EnsureSuccessStatusCode();
        var itemType = await createItemTypeResponse.Content.ReadFromJsonAsync<ItemTypeResponse>();
        Assert.NotNull(itemType);

        var createCategoryResponse = await _httpClient.PostAsJsonAsync("/api/categories", new CreateCategoryRequest("Basic"));
        Assert.Equal(HttpStatusCode.Created, createCategoryResponse.StatusCode);
        Assert.NotNull(createCategoryResponse.Headers.Location);

        var createdCategory = await createCategoryResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(createdCategory);
        Assert.Equal("Basic", createdCategory.Name);
        Assert.Empty(createdCategory.Items);

        var addItemResponse = await _httpClient.PostAsJsonAsync(
            $"/api/categories/{createdCategory.Id}/items",
            new AddCategoryItemRequest(itemType.Id, 2));

        Assert.Equal(HttpStatusCode.OK, addItemResponse.StatusCode);
        var updatedCategory = await addItemResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(updatedCategory);
        Assert.Equal(createdCategory.Id, updatedCategory.Id);
        Assert.Single(updatedCategory.Items);
        Assert.Equal(itemType.Id, updatedCategory.Items[0].ItemTypeId);
        Assert.Equal(2, updatedCategory.Items[0].Quantity);

        var getByIdResponse = await _httpClient.GetAsync($"/api/categories/{createdCategory.Id}");
        Assert.Equal(HttpStatusCode.OK, getByIdResponse.StatusCode);
        var fetchedCategory = await getByIdResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        Assert.NotNull(fetchedCategory);
        Assert.Equal(createdCategory.Id, fetchedCategory.Id);
        Assert.Single(fetchedCategory.Items);
    }

    [Fact]
    public async Task Create_endpoint_should_return_forbidden_for_viewer_role()
    {
        await AuthenticateAsAsync("viewer", "viewer123");

        var response = await _httpClient.PostAsJsonAsync("/api/kits", new CreateKitRequest("Viewer Theme"));

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        Assert.Equal("forbidden", await ReadProblemCodeAsync(response));
    }

    [Fact]
    public async Task Reserve_endpoint_should_create_reservation_when_stock_is_available()
    {
        await AuthenticateAsManagerAsync();

        var categoryId = await CreateCategoryWithItemAsync("Basic", "Panel", 10, 2);
        var kitId = await CreateKitAsync("Paw Patrol");

        var response = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest(categoryId, "2026-03-10", "2026-03-12"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var reserveResponse = await response.Content.ReadFromJsonAsync<ReserveKitResponse>();
        Assert.NotNull(reserveResponse);
        Assert.Equal(kitId, reserveResponse.KitThemeId);
        Assert.Equal(categoryId, reserveResponse.KitCategoryId);
        Assert.Equal("Active", reserveResponse.Status);
        Assert.Equal("Cliente Teste", reserveResponse.CustomerName);
        Assert.Equal("12345678900", reserveResponse.CustomerDocumentNumber);
        Assert.Equal("12999990000", reserveResponse.CustomerPhoneNumber);
        Assert.False(reserveResponse.IsEntryPaid);

        var reservationsResponse = await _httpClient.GetAsync($"/api/kits/{kitId}/reservations");
        var reservations = await reservationsResponse.Content.ReadFromJsonAsync<List<ReservationResponse>>();

        Assert.Equal(HttpStatusCode.OK, reservationsResponse.StatusCode);
        Assert.NotNull(reservations);
        Assert.Single(reservations);
        Assert.Equal(categoryId, reservations[0].KitCategoryId);
        Assert.Equal("Cliente Teste", reservations[0].CustomerName);
        Assert.Equal("12999990000", reservations[0].CustomerPhoneNumber);
    }

    [Fact]
    public async Task Reserve_endpoint_should_return_conflict_when_stock_is_insufficient()
    {
        await AuthenticateAsManagerAsync();

        var categoryId = await CreateCategoryWithItemAsync("Complete", "Balloon", 5, 3);
        var firstThemeId = await CreateKitAsync("Paw Patrol");
        var secondThemeId = await CreateKitAsync("Frozen");

        var firstReserveResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{firstThemeId}/reservations",
            new ReserveKitRequest(categoryId, "2026-04-10", "2026-04-12"));
        firstReserveResponse.EnsureSuccessStatusCode();

        var conflictResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{secondThemeId}/reservations",
            new ReserveKitRequest(categoryId, "2026-04-11", "2026-04-13"));

        Assert.Equal(HttpStatusCode.Conflict, conflictResponse.StatusCode);
        Assert.Equal("conflict", await ReadProblemCodeAsync(conflictResponse));
    }

    [Fact]
    public async Task Reserve_endpoint_should_allow_non_overlapping_period_with_same_item_stock()
    {
        await AuthenticateAsManagerAsync();

        var categoryId = await CreateCategoryWithItemAsync("Basic", "Roman Panel", 1, 1);
        var firstThemeId = await CreateKitAsync("Turma da Monica");
        var secondThemeId = await CreateKitAsync("Paw Patrol");

        var firstReserveResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{firstThemeId}/reservations",
            new ReserveKitRequest(categoryId, "2026-02-22", "2026-02-24"));
        firstReserveResponse.EnsureSuccessStatusCode();

        var secondReserveResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{secondThemeId}/reservations",
            new ReserveKitRequest(categoryId, "2026-02-25", "2026-02-26"));
        secondReserveResponse.EnsureSuccessStatusCode();

        var secondReservePayload = await secondReserveResponse.Content.ReadFromJsonAsync<ReserveKitResponse>();
        Assert.NotNull(secondReservePayload);
        Assert.False(secondReservePayload.IsStockOverride);
        Assert.Null(secondReservePayload.StockOverrideReason);
    }

    [Fact]
    public async Task Reserve_endpoint_should_allow_reservation_when_periods_only_touch_at_boundary()
    {
        await AuthenticateAsManagerAsync();

        var categoryId = await CreateCategoryWithItemAsync("Basic", "Roman Arc", 1, 1);
        var firstThemeId = await CreateKitAsync("Turma da Monica");
        var secondThemeId = await CreateKitAsync("Paw Patrol");

        var firstReserveResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{firstThemeId}/reservations",
            new ReserveKitRequest(categoryId, "2026-02-22", "2026-02-24"));
        firstReserveResponse.EnsureSuccessStatusCode();

        var secondReserveResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{secondThemeId}/reservations",
            new ReserveKitRequest(categoryId, "2026-02-24", "2026-02-26"));
        secondReserveResponse.EnsureSuccessStatusCode();

        var secondReservePayload = await secondReserveResponse.Content.ReadFromJsonAsync<ReserveKitResponse>();
        Assert.NotNull(secondReservePayload);
        Assert.False(secondReservePayload.IsStockOverride);
        Assert.Null(secondReservePayload.StockOverrideReason);
    }

    [Fact]
    public async Task Reserve_endpoint_should_allow_stock_override_when_reason_is_provided()
    {
        await AuthenticateAsManagerAsync();

        var categoryId = await CreateCategoryWithItemAsync("Complete", "Roman Arch", 1, 1);
        var firstThemeId = await CreateKitAsync("Turma da Monica");
        var secondThemeId = await CreateKitAsync("Frozen");

        var firstReserveResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{firstThemeId}/reservations",
            new ReserveKitRequest(categoryId, "2026-03-10", "2026-03-12"));
        firstReserveResponse.EnsureSuccessStatusCode();

        var overrideReserveResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{secondThemeId}/reservations",
            new ReserveKitRequest(
                categoryId,
                "2026-03-11",
                "2026-03-12",
                true,
                "Aprovado para cliente recorrente e montagem prioritária."));

        Assert.Equal(HttpStatusCode.OK, overrideReserveResponse.StatusCode);

        var overridePayload = await overrideReserveResponse.Content.ReadFromJsonAsync<ReserveKitResponse>();
        Assert.NotNull(overridePayload);
        Assert.True(overridePayload.IsStockOverride);
        Assert.Equal("Aprovado para cliente recorrente e montagem prioritária.", overridePayload.StockOverrideReason);
    }

    [Fact]
    public async Task Reserve_endpoint_should_return_bad_request_when_end_date_is_before_start_date()
    {
        await AuthenticateAsManagerAsync();

        var categoryId = await CreateCategoryWithItemAsync("Intermediate", "Table", 6, 1);
        var kitId = await CreateKitAsync("Safari");

        var invalidResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest(categoryId, "2026-06-10", "2026-06-09"));

        Assert.Equal(HttpStatusCode.BadRequest, invalidResponse.StatusCode);
        Assert.Equal("validation_error", await ReadProblemCodeAsync(invalidResponse));
        Assert.True(await ReadValidationErrorsCountAsync(invalidResponse) > 0);
    }

    [Fact]
    public async Task Get_kits_endpoint_should_return_paginated_result()
    {
        await AuthenticateAsManagerAsync();

        await CreateKitAsync("Paged Theme A");
        await CreateKitAsync("Paged Theme B");
        await CreateKitAsync("Paged Theme C");

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
    public async Task Cancel_endpoint_should_release_items_for_new_reservation()
    {
        await AuthenticateAsManagerAsync();

        var categoryId = await CreateCategoryWithItemAsync("Starter", "Arch", 4, 2);
        var firstThemeId = await CreateKitAsync("Paw Patrol");
        var secondThemeId = await CreateKitAsync("Mickey");

        await _httpClient.PostAsJsonAsync(
            $"/api/kits/{firstThemeId}/reservations",
            new ReserveKitRequest(categoryId, "2026-05-01", "2026-05-03"));

        var reservationsResponse = await _httpClient.GetAsync($"/api/kits/{firstThemeId}/reservations");
        var reservations = await reservationsResponse.Content.ReadFromJsonAsync<List<ReservationResponse>>();

        Assert.NotNull(reservations);
        var reservationId = reservations[0].Id;

        var cancelResponse = await _httpClient.PostAsync(
            $"/api/kits/{firstThemeId}/reservations/{reservationId}/cancel",
            content: null);

        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        var reserveAgainResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{secondThemeId}/reservations",
            new ReserveKitRequest(categoryId, "2026-05-01", "2026-05-03"));

        Assert.Equal(HttpStatusCode.OK, reserveAgainResponse.StatusCode);
    }

    [Fact]
    public async Task Update_reservation_endpoint_should_update_payload_and_keep_same_reservation_id()
    {
        await AuthenticateAsManagerAsync();

        var categoryId = await CreateCategoryWithItemAsync("Completo", "Painel", 1, 1);
        var kitId = await CreateKitAsync("Tema Teste");

        var createReservationResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest(categoryId, "2026-07-10", "2026-07-12"));
        createReservationResponse.EnsureSuccessStatusCode();

        var createdReservation = await createReservationResponse.Content.ReadFromJsonAsync<ReserveKitResponse>();
        Assert.NotNull(createdReservation);

        var updateResponse = await _httpClient.PutAsJsonAsync(
            $"/api/kits/{kitId}/reservations/{createdReservation.ReservationId}",
            new UpdateReservationRequest(
                categoryId,
                "2026-07-11",
                "2026-07-13",
                false,
                null,
                "Cliente Editado",
                "99988877766",
                "11991112222",
                "Rua Atualizada, 77",
                "Atualizacao de teste.",
                true,
                true));

        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedReservation = await updateResponse.Content.ReadFromJsonAsync<UpdateReservationResponse>();
        Assert.NotNull(updatedReservation);
        Assert.Equal(createdReservation.ReservationId, updatedReservation.ReservationId);
        Assert.Equal("Cliente Editado", updatedReservation.CustomerName);
        Assert.Equal("99988877766", updatedReservation.CustomerDocumentNumber);
        Assert.Equal("11991112222", updatedReservation.CustomerPhoneNumber);
        Assert.Equal("Rua Atualizada, 77", updatedReservation.CustomerAddress);
        Assert.Equal("Atualizacao de teste.", updatedReservation.Notes);
        Assert.True(updatedReservation.HasBalloonArch);
        Assert.True(updatedReservation.IsEntryPaid);
    }

    private async Task<Guid> CreateKitAsync(string name)
    {
        var response = await _httpClient.PostAsJsonAsync("/api/kits", new CreateKitRequest(name));
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<KitSummaryResponse>();
        if (body is null)
        {
            throw new InvalidOperationException("Create theme response body is null.");
        }

        return body.Id;
    }

    private async Task<Guid> CreateCategoryWithItemAsync(string categoryName, string itemName, int stock, int quantity)
    {
        var uniqueItemName = $"{itemName}-{Guid.NewGuid():N}".Substring(0, 18);
        var itemTypeResponse = await _httpClient.PostAsJsonAsync("/api/item-types", new CreateItemTypeRequest(uniqueItemName, stock));
        itemTypeResponse.EnsureSuccessStatusCode();
        var itemType = await itemTypeResponse.Content.ReadFromJsonAsync<ItemTypeResponse>();
        if (itemType is null)
        {
            throw new InvalidOperationException("Create item type response body is null.");
        }

        var categoryResponse = await _httpClient.PostAsJsonAsync("/api/categories", new CreateCategoryRequest(categoryName));
        categoryResponse.EnsureSuccessStatusCode();
        var category = await categoryResponse.Content.ReadFromJsonAsync<CategoryResponse>();
        if (category is null)
        {
            throw new InvalidOperationException("Create category response body is null.");
        }

        var addItemResponse = await _httpClient.PostAsJsonAsync(
            $"/api/categories/{category.Id}/items",
            new AddCategoryItemRequest(itemType.Id, quantity));
        addItemResponse.EnsureSuccessStatusCode();

        return category.Id;
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

    private sealed record ReserveKitRequest(
        Guid KitCategoryId,
        string StartDate,
        string EndDate,
        bool AllowStockOverride = false,
        string? StockOverrideReason = null,
        string CustomerName = "Cliente Teste",
        string CustomerDocumentNumber = "12345678900",
        string CustomerPhoneNumber = "12999990000",
        string CustomerAddress = "Rua Teste, 100",
        string? Notes = "Reserva criada por teste automatizado.",
        bool HasBalloonArch = false,
        bool IsEntryPaid = false);

    private sealed record UpdateReservationRequest(
        Guid KitCategoryId,
        string StartDate,
        string EndDate,
        bool AllowStockOverride = false,
        string? StockOverrideReason = null,
        string CustomerName = "Cliente Teste",
        string CustomerDocumentNumber = "12345678900",
        string CustomerPhoneNumber = "12999990000",
        string CustomerAddress = "Rua Teste, 100",
        string? Notes = "Reserva atualizada por teste automatizado.",
        bool HasBalloonArch = false,
        bool IsEntryPaid = false);

    private sealed record AuthTokenRequest(string Username, string Password);

    private sealed record AuthTokenResponse(string AccessToken);

    private sealed record CreateItemTypeRequest(string Name, int TotalStock);

    private sealed record UpdateItemStockRequest(int TotalStock);

    private sealed record ItemTypeResponse(Guid Id, string Name, int TotalStock);

    private sealed record CreateCategoryRequest(string Name);

    private sealed record AddCategoryItemRequest(Guid ItemTypeId, int Quantity);

    private sealed record CategoryItemResponse(Guid ItemTypeId, int Quantity);

    private sealed record CategoryResponse(Guid Id, string Name, IReadOnlyList<CategoryItemResponse> Items);

    private sealed record KitSummaryResponse(Guid Id, string Name);

    private sealed record ReservationResponse(
        Guid Id,
        Guid KitCategoryId,
        string StartDate,
        string EndDate,
        string Status,
        bool IsStockOverride,
        string? StockOverrideReason,
        string CustomerName,
        string CustomerDocumentNumber,
        string CustomerPhoneNumber,
        string CustomerAddress,
        string? Notes,
        bool HasBalloonArch,
        bool IsEntryPaid);

    private sealed record ReserveKitResponse(
        Guid ReservationId,
        Guid KitThemeId,
        Guid KitCategoryId,
        string StartDate,
        string EndDate,
        string Status,
        bool IsStockOverride,
        string? StockOverrideReason,
        string CustomerName,
        string CustomerDocumentNumber,
        string CustomerPhoneNumber,
        string CustomerAddress,
        string? Notes,
        bool HasBalloonArch,
        bool IsEntryPaid,
        string Message);

    private sealed record UpdateReservationResponse(
        Guid ReservationId,
        Guid KitThemeId,
        Guid KitCategoryId,
        string StartDate,
        string EndDate,
        string Status,
        bool IsStockOverride,
        string? StockOverrideReason,
        string CustomerName,
        string CustomerDocumentNumber,
        string CustomerPhoneNumber,
        string CustomerAddress,
        string? Notes,
        bool HasBalloonArch,
        bool IsEntryPaid,
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
