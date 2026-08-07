using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Testcontainers.PostgreSql;

namespace ThrottleWatch.Api.Tests;

public sealed class ThrottleWatchApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string TestApiKey = "test-throttlewatch-key";
    public const string TestApiKeyHeader = "X-ThrottleWatch-Key";

    private PostgreSqlContainer? _postgres;
    private string _connectionString = string.Empty;

    public async Task InitializeAsync()
    {
        if (await TryStartContainerAsync())
            return;

        await UseLocalPostgresAsync();
    }

    private async Task<bool> TryStartContainerAsync()
    {
        try
        {
            _postgres = new PostgreSqlBuilder()
                .WithImage("postgres:16-alpine")
                .WithDatabase("throttlewatch_tests")
                .WithUsername("postgres")
                .WithPassword("postgres")
                .Build();

            await _postgres.StartAsync();
            _connectionString = _postgres.GetConnectionString();
            return true;
        }
        catch
        {
            if (_postgres is not null)
            {
                await _postgres.DisposeAsync();
                _postgres = null;
            }

            return false;
        }
    }

    private async Task UseLocalPostgresAsync()
    {
        var dbName = $"throttlewatch_tests_{Guid.NewGuid():N}";
        var adminCs = "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=postgres";

        await using (var admin = new NpgsqlConnection(adminCs))
        {
            await admin.OpenAsync();
            await using var cmd = new NpgsqlCommand($"CREATE DATABASE \"{dbName}\";", admin);
            await cmd.ExecuteNonQueryAsync();
        }

        _connectionString =
            $"Host=localhost;Port=5432;Database={dbName};Username=postgres;Password=postgres";
    }

    public new async Task DisposeAsync()
    {
        if (_postgres is not null)
            await _postgres.DisposeAsync();

        await base.DisposeAsync();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _connectionString,
                ["Database:ApplyMigrationsOnStartup"] = "true",
                ["ThrottleWatch:Security:ApiKey"] = TestApiKey,
                ["ThrottleWatch:Security:HeaderName"] = TestApiKeyHeader,
                ["ThrottleWatch:Alerts:Enabled"] = "false",
                ["ThrottleWatch:Insights:IntervalMinutes"] = "60",
                ["Serilog:MinimumLevel:Default"] = "Warning"
            });
        });

        builder.ConfigureServices(services =>
        {
            var hosted = services.Where(d => d.ServiceType == typeof(IHostedService)).ToList();
            foreach (var descriptor in hosted)
                services.Remove(descriptor);
        });
    }

    public HttpClient CreateAuthenticatedClient()
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation(TestApiKeyHeader, TestApiKey);
        return client;
    }
}
