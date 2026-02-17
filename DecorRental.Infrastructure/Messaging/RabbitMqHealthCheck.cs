using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DecorRental.Infrastructure.Messaging;

public sealed class RabbitMqHealthCheck : IHealthCheck
{
    private readonly RabbitMqOptions _options;

    public RabbitMqHealthCheck(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
    }

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_options.Enabled)
        {
            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ disabled."));
        }

        try
        {
            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            using var connection = factory.CreateConnection();

            return connection.IsOpen
                ? Task.FromResult(HealthCheckResult.Healthy("RabbitMQ reachable."))
                : Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ connection is closed."));
        }
        catch (Exception exception)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy("RabbitMQ unreachable.", exception));
        }
    }
}
