using System.Net;
using Xunit;

namespace DecorRental.Tests.Integration;

public sealed class ObservabilityApiTests : IClassFixture<DecorRentalApiFactory>
{
    private readonly HttpClient _httpClient;

    public ObservabilityApiTests(DecorRentalApiFactory factory)
    {
        _httpClient = factory.CreateClient();
    }

    [Fact]
    public async Task Health_endpoint_should_report_healthy()
    {
        var response = await _httpClient.GetAsync("/health");
        var payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", payload, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Metrics_endpoint_should_expose_prometheus_metrics()
    {
        var response = await _httpClient.GetAsync("/metrics");
        var payload = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("# HELP", payload, StringComparison.Ordinal);
    }
}
