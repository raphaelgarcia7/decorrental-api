using System.Text.Json;
using System.Text.Json.Serialization;
using DecorRental.Application.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace DecorRental.Infrastructure.Messaging;

public sealed class RabbitMqMessageBus : IMessageBus
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly RabbitMqOptions _options;
    private readonly ILogger<RabbitMqMessageBus> _logger;

    public RabbitMqMessageBus(IOptions<RabbitMqOptions> options, ILogger<RabbitMqMessageBus> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent
    {
        if (!_options.Enabled)
        {
            _logger.LogInformation("RabbitMQ publishing disabled. Event {EventType} skipped.", typeof(TEvent).Name);
            return Task.CompletedTask;
        }

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.UserName,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            using var connection = factory.CreateConnection();
            using var channel = connection.CreateModel();

            channel.ExchangeDeclare(
                exchange: _options.ExchangeName,
                type: ExchangeType.Topic,
                durable: true);

            var payload = JsonSerializer.SerializeToUtf8Bytes(integrationEvent, SerializerOptions);
            var properties = channel.CreateBasicProperties();
            properties.ContentType = "application/json";
            properties.DeliveryMode = 2;
            properties.MessageId = integrationEvent.EventId.ToString();
            properties.Timestamp = new AmqpTimestamp(integrationEvent.OccurredAtUtc.ToUnixTimeSeconds());

            channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: integrationEvent.RoutingKey,
                basicProperties: properties,
                body: payload);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to publish integration event {EventType} via RabbitMQ.",
                typeof(TEvent).Name);
        }

        return Task.CompletedTask;
    }
}
