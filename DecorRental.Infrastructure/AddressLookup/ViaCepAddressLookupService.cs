using System.Net;
using System.Net.Http.Json;
using DecorRental.Application.Contracts;
using Microsoft.Extensions.Logging;

namespace DecorRental.Infrastructure.AddressLookup;

public sealed class ViaCepAddressLookupService : IAddressLookupService
{
    private const string BrasilApiBaseAddress = "https://brasilapi.com.br/api/cep/v1/";

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

        var viaCepAttempt = await TryLookupViaCepAsync(normalizedZipCode, cancellationToken);
        if (viaCepAttempt.Result is not null)
        {
            return viaCepAttempt.Result;
        }

        var brasilApiAttempt = await TryLookupBrasilApiAsync(normalizedZipCode, cancellationToken);
        if (brasilApiAttempt.Result is not null)
        {
            return brasilApiAttempt.Result;
        }

        if (viaCepAttempt.IsUnavailable && brasilApiAttempt.IsUnavailable)
        {
            throw new HttpRequestException("Servicos de CEP indisponiveis.");
        }

        return null;
    }

    private async Task<LookupAttempt> TryLookupViaCepAsync(string zipCode, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"{zipCode}/json/", cancellationToken);
            if (response.StatusCode is HttpStatusCode.NotFound)
            {
                return LookupAttempt.NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Consulta ViaCEP para {ZipCode} retornou status {StatusCode}.",
                    zipCode,
                    (int)response.StatusCode);
                return LookupAttempt.Unavailable();
            }

            var payload = await response.Content.ReadFromJsonAsync<ViaCepResponse>(cancellationToken: cancellationToken);
            if (payload is null || payload.Erro)
            {
                return LookupAttempt.NotFound();
            }

            if (string.IsNullOrWhiteSpace(payload.Logradouro) ||
                string.IsNullOrWhiteSpace(payload.Bairro) ||
                string.IsNullOrWhiteSpace(payload.Localidade) ||
                string.IsNullOrWhiteSpace(payload.Uf))
            {
                _logger.LogWarning("CEP {ZipCode} retornou dados incompletos no ViaCEP.", zipCode);
            }

            return LookupAttempt.Success(
                new AddressLookupResult(
                    zipCode,
                    payload.Logradouro?.Trim() ?? string.Empty,
                    payload.Bairro?.Trim() ?? string.Empty,
                    payload.Localidade?.Trim() ?? string.Empty,
                    payload.Uf?.Trim().ToUpperInvariant() ?? string.Empty));
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Falha de rede ao consultar ViaCEP para {ZipCode}.", zipCode);
            return LookupAttempt.Unavailable();
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogWarning(exception, "Timeout ao consultar ViaCEP para {ZipCode}.", zipCode);
            return LookupAttempt.Unavailable();
        }
    }

    private async Task<LookupAttempt> TryLookupBrasilApiAsync(string zipCode, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient.GetAsync($"{BrasilApiBaseAddress}{zipCode}", cancellationToken);
            if (response.StatusCode is HttpStatusCode.NotFound)
            {
                return LookupAttempt.NotFound();
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "Consulta BrasilAPI para {ZipCode} retornou status {StatusCode}.",
                    zipCode,
                    (int)response.StatusCode);
                return LookupAttempt.Unavailable();
            }

            var payload = await response.Content.ReadFromJsonAsync<BrasilApiResponse>(cancellationToken: cancellationToken);
            if (payload is null)
            {
                return LookupAttempt.NotFound();
            }

            return LookupAttempt.Success(
                new AddressLookupResult(
                    zipCode,
                    payload.Street?.Trim() ?? string.Empty,
                    payload.Neighborhood?.Trim() ?? string.Empty,
                    payload.City?.Trim() ?? string.Empty,
                    payload.State?.Trim().ToUpperInvariant() ?? string.Empty));
        }
        catch (HttpRequestException exception)
        {
            _logger.LogWarning(exception, "Falha de rede ao consultar BrasilAPI para {ZipCode}.", zipCode);
            return LookupAttempt.Unavailable();
        }
        catch (TaskCanceledException exception)
        {
            _logger.LogWarning(exception, "Timeout ao consultar BrasilAPI para {ZipCode}.", zipCode);
            return LookupAttempt.Unavailable();
        }
    }

    private sealed record ViaCepResponse(
        string? Cep,
        string? Logradouro,
        string? Bairro,
        string? Localidade,
        string? Uf,
        bool Erro = false);

    private sealed record BrasilApiResponse(
        string? Cep,
        string? State,
        string? City,
        string? Neighborhood,
        string? Street);

    private sealed record LookupAttempt(AddressLookupResult? Result, bool IsUnavailable)
    {
        public static LookupAttempt Success(AddressLookupResult result) => new(result, IsUnavailable: false);
        public static LookupAttempt NotFound() => new(null, IsUnavailable: false);
        public static LookupAttempt Unavailable() => new(null, IsUnavailable: true);
    }
}
