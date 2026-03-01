using System.Net;
using System.Net.Http.Json;
using DecorRental.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace DecorRental.Infrastructure.AddressLookup;

public sealed class ViaCepAddressLookupService : IAddressLookupService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ViaCepAddressLookupService> _logger;

    public ViaCepAddressLookupService(HttpClient httpClient, ILogger<ViaCepAddressLookupService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<AddressLookupResult?> LookupByZipCodeAsync(string zipCode, CancellationToken cancellationToken = default)
    {
        var normalizedZipCode = new string(zipCode.Where(char.IsDigit).ToArray());
        if (normalizedZipCode.Length != 8)
        {
            return null;
        }

        using var response = await _httpClient.GetAsync($"{normalizedZipCode}/json/", cancellationToken);
        if (response.StatusCode is HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var payload = await response.Content.ReadFromJsonAsync<ViaCepResponse>(cancellationToken: cancellationToken);
        if (payload is null || payload.Erro)
        {
            return null;
        }

        if (string.IsNullOrWhiteSpace(payload.Logradouro) ||
            string.IsNullOrWhiteSpace(payload.Bairro) ||
            string.IsNullOrWhiteSpace(payload.Localidade) ||
            string.IsNullOrWhiteSpace(payload.Uf))
        {
            _logger.LogWarning("CEP {ZipCode} retornou dados incompletos no ViaCEP.", normalizedZipCode);
        }

        return new AddressLookupResult(
            normalizedZipCode,
            payload.Logradouro?.Trim() ?? string.Empty,
            payload.Bairro?.Trim() ?? string.Empty,
            payload.Localidade?.Trim() ?? string.Empty,
            payload.Uf?.Trim().ToUpperInvariant() ?? string.Empty);
    }

    private sealed record ViaCepResponse(
        string? Cep,
        string? Logradouro,
        string? Bairro,
        string? Localidade,
        string? Uf,
        bool Erro = false);
}
