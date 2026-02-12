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
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={_databaseFileName}"
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
