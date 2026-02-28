using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Xunit;

namespace DecorRental.Tests.Integration;

public sealed class ContractsApiTests : IClassFixture<DecorRentalApiFactory>
{
    private readonly HttpClient _httpClient;

    public ContractsApiTests(DecorRentalApiFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Get_contract_data_endpoint_should_return_prefilled_payload_from_reservation()
    {
        await AuthenticateAsManagerAsync();

        var categoryId = await CreateCategoryWithItemAsync("Basica", "Painel", 3, 1);
        var kitId = await CreateKitAsync("Turma da Monica");

        var reserveResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest(
                categoryId,
                "2026-04-01",
                "2026-04-03",
                CustomerName: "Bruna Comenali",
                CustomerDocumentNumber: "40825890829",
                CustomerPhoneNumber: "12999990000",
                CustomerAddress: "Av Pedro Friggi, 3100",
                Notes: "Montagem no periodo da manha.",
                HasBalloonArch: true,
                IsEntryPaid: true));

        reserveResponse.EnsureSuccessStatusCode();
        var reservePayload = await reserveResponse.Content.ReadFromJsonAsync<ReserveKitResponse>();
        Assert.NotNull(reservePayload);

        var contractDataResponse = await _httpClient.GetAsync(
            $"/api/kits/{kitId}/reservations/{reservePayload.ReservationId}/contract-data");

        Assert.Equal(HttpStatusCode.OK, contractDataResponse.StatusCode);

        var contractData = await contractDataResponse.Content.ReadFromJsonAsync<ContractDataResponse>();
        Assert.NotNull(contractData);
        Assert.Equal("Bruna Comenali", contractData.CustomerName);
        Assert.Equal("40825890829", contractData.CustomerDocumentNumber);
        Assert.Equal("12999990000", contractData.CustomerPhoneNumber);
        Assert.Equal("Av Pedro Friggi, 3100", contractData.CustomerAddress);
        Assert.Equal("Turma da Monica", contractData.KitThemeName);
        Assert.Equal("Basica", contractData.KitCategoryName);
        Assert.True(contractData.HasBalloonArch);
        Assert.True(contractData.IsEntryPaid);
    }

    [Fact]
    public async Task Generate_contract_endpoint_should_return_docx_and_pdf_files()
    {
        await AuthenticateAsManagerAsync();

        var categoryId = await CreateCategoryWithItemAsync("Intermediaria", "Mesa", 5, 1);
        var kitId = await CreateKitAsync("Safari");

        var reserveResponse = await _httpClient.PostAsJsonAsync(
            $"/api/kits/{kitId}/reservations",
            new ReserveKitRequest(
                categoryId,
                "2026-05-10",
                "2026-05-12",
                CustomerName: "Cliente Contrato",
                CustomerDocumentNumber: "12345678900",
                CustomerPhoneNumber: "11999998888",
                CustomerAddress: "Rua Teste, 100",
                Notes: "Contrato para validacao de arquivo.",
                HasBalloonArch: false,
                IsEntryPaid: false));

        reserveResponse.EnsureSuccessStatusCode();
        var reservePayload = await reserveResponse.Content.ReadFromJsonAsync<ReserveKitResponse>();
        Assert.NotNull(reservePayload);

        var contractDataResponse = await _httpClient.GetAsync(
            $"/api/kits/{kitId}/reservations/{reservePayload.ReservationId}/contract-data");
        contractDataResponse.EnsureSuccessStatusCode();

        var contractData = await contractDataResponse.Content.ReadFromJsonAsync<ContractDataResponse>();
        Assert.NotNull(contractData);

        var request = new ContractDataRequest(
            contractData.KitThemeId,
            contractData.ReservationId,
            contractData.KitThemeName,
            contractData.KitCategoryName,
            contractData.ReservationStartDate,
            contractData.ReservationEndDate,
            contractData.CustomerName,
            contractData.CustomerDocumentNumber,
            contractData.CustomerPhoneNumber,
            contractData.CustomerAddress,
            contractData.Notes,
            contractData.HasBalloonArch,
            contractData.IsEntryPaid,
            contractData.ContractDate);

        var docxResponse = await _httpClient.PostAsJsonAsync("/api/contracts/generate?format=docx", request);
        Assert.Equal(HttpStatusCode.OK, docxResponse.StatusCode);
        Assert.Equal(
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            docxResponse.Content.Headers.ContentType?.MediaType);
        var docxBytes = await docxResponse.Content.ReadAsByteArrayAsync();
        Assert.True(docxBytes.Length > 500);

        var pdfResponse = await _httpClient.PostAsJsonAsync("/api/contracts/generate?format=pdf", request);
        Assert.Equal(HttpStatusCode.OK, pdfResponse.StatusCode);
        Assert.Equal("application/pdf", pdfResponse.Content.Headers.ContentType?.MediaType);
        var pdfBytes = await pdfResponse.Content.ReadAsByteArrayAsync();
        Assert.True(pdfBytes.Length > 200);
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

    private sealed record AuthTokenRequest(string Username, string Password);

    private sealed record AuthTokenResponse(string AccessToken);

    private sealed record CreateKitRequest(string Name);

    private sealed record KitSummaryResponse(Guid Id, string Name);

    private sealed record CreateCategoryRequest(string Name);

    private sealed record CategoryResponse(Guid Id, string Name);

    private sealed record CreateItemTypeRequest(string Name, int TotalStock);

    private sealed record ItemTypeResponse(Guid Id, string Name, int TotalStock);

    private sealed record AddCategoryItemRequest(Guid ItemTypeId, int Quantity);

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

    private sealed record ReserveKitResponse(Guid ReservationId);

    private sealed record ContractDataResponse(
        Guid KitThemeId,
        Guid ReservationId,
        string KitThemeName,
        string KitCategoryName,
        string ReservationStartDate,
        string ReservationEndDate,
        string CustomerName,
        string CustomerDocumentNumber,
        string CustomerPhoneNumber,
        string CustomerAddress,
        string? Notes,
        bool HasBalloonArch,
        bool IsEntryPaid,
        string ContractDate);

    private sealed record ContractDataRequest(
        Guid KitThemeId,
        Guid ReservationId,
        string KitThemeName,
        string KitCategoryName,
        string ReservationStartDate,
        string ReservationEndDate,
        string CustomerName,
        string CustomerDocumentNumber,
        string CustomerPhoneNumber,
        string CustomerAddress,
        string? Notes,
        bool HasBalloonArch,
        bool IsEntryPaid,
        string ContractDate);
}
