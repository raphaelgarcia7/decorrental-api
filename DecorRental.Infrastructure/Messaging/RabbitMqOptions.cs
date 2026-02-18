namespace DecorRental.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public bool Enabled { get; set; } = true;
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangeName { get; set; } = "decorental.events";

    public string BuildConnectionString()
    {
        var vhost = string.IsNullOrWhiteSpace(VirtualHost) ? "/" : VirtualHost.Trim();
        var vhostPath = vhost == "/" ? string.Empty : $"/{vhost.TrimStart('/')}";

        return $"amqp://{Uri.EscapeDataString(UserName)}:{Uri.EscapeDataString(Password)}@{HostName}:{Port}{vhostPath}";
    }
}
