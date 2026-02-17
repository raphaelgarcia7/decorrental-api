using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace DecorRental.Tests.Integration;

public sealed class DecorRentalApiFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseFileName = $"decorental-integration-{Guid.NewGuid():N}.db";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, configurationBuilder) =>
        {
            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databaseFileName}",
                ["Jwt:Issuer"] = "DecorRental.Api.Tests",
                ["Jwt:Audience"] = "DecorRental.Tests",
                ["Jwt:SigningKey"] = "DecorRental-Tests-Only-Signing-Key-Should-Be-Long",
                ["Jwt:TokenExpirationMinutes"] = "60",
                ["RabbitMq:Enabled"] = "false",
                ["Jwt:Users:0:Username"] = "viewer",
                ["Jwt:Users:0:Password"] = "viewer123",
                ["Jwt:Users:0:Role"] = "Viewer",
                ["Jwt:Users:1:Username"] = "manager",
                ["Jwt:Users:1:Password"] = "manager123",
                ["Jwt:Users:1:Role"] = "Manager",
                ["Jwt:Users:2:Username"] = "admin",
                ["Jwt:Users:2:Password"] = "admin123",
                ["Jwt:Users:2:Role"] = "Admin"
            });
        });
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();

        var paths = new[]
        {
            _databaseFileName,
            $"{_databaseFileName}-shm",
            $"{_databaseFileName}-wal"
        };

        foreach (var path in paths)
        {
            DeleteFileWithRetry(path);
        }
    }

    private static void DeleteFileWithRetry(string path)
    {
        const int maxAttempts = 5;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }

                return;
            }
            catch (IOException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100);
            }
            catch (UnauthorizedAccessException) when (attempt < maxAttempts)
            {
                Thread.Sleep(100);
            }
            catch (Exception)
            {
                return;
            }
        }
    }
}
