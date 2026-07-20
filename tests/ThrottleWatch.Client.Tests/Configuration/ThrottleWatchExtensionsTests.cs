using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ThrottleWatch.Client.Configuration;
using ThrottleWatch.Client.Http;
using ThrottleWatch.Client.Queue;

namespace ThrottleWatch.Client.Tests.Configuration;

public sealed class ThrottleWatchExtensionsTests
{
    [Fact]
    public void AddThrottleWatch_WithAction_ShouldRegisterBuffer_HttpClient_AndMetricSender()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddThrottleWatch(o => o.ApiBaseUrl = "http://localhost:5287");

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<LocalMetricBuffer>().Should().NotBeNull();
        provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(MetricSender.HttpClientName)
            .BaseAddress.Should().Be(new Uri("http://localhost:5287/"));
        provider.GetServices<IHostedService>().Should().ContainSingle(s => s is MetricSender);
    }

    [Fact]
    public void AddThrottleWatch_WithConfigSectionPath_ShouldBindConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ThrottleWatchOptions.SectionName}:ApiBaseUrl"] = "http://localhost:5287"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddThrottleWatch(ThrottleWatchOptions.SectionName);

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IOptions<ThrottleWatchOptions>>().Value.ApiBaseUrl
            .Should().Be("http://localhost:5287");
        provider.GetRequiredService<IHttpClientFactory>()
            .CreateClient(MetricSender.HttpClientName)
            .BaseAddress.Should().Be(new Uri("http://localhost:5287/"));
    }

    [Fact]
    public void AddThrottleWatch_WithNamedConfigurationSection_ShouldBindSection()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [$"{ThrottleWatchOptions.SectionName}:ApiBaseUrl"] = "http://localhost:5287"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddThrottleWatch(configuration.GetSection(ThrottleWatchOptions.SectionName));

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IOptions<ThrottleWatchOptions>>().Value.ApiBaseUrl
            .Should().Be("http://localhost:5287");
    }

    [Fact]
    public void AddThrottleWatch_WithInvalidApiBaseUrl_ShouldFailOnOptionsValidation()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddThrottleWatch(o => o.ApiBaseUrl = "not-a-url");

        using var provider = services.BuildServiceProvider();

        var act = () => provider.GetRequiredService<LocalMetricBuffer>();

        act.Should().Throw<OptionsValidationException>();
    }

    [Fact]
    public async Task UseThrottleWatch_WithTestServer_ShouldCaptureRequest()
    {
        using var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder =>
            {
                webBuilder
                    .UseTestServer()
                    .ConfigureServices(services =>
                    {
                        services.AddLogging();
                        services.AddThrottleWatch(o =>
                        {
                            o.ApiBaseUrl = "http://localhost:5287";
                            o.FlushIntervalMilliseconds = 60_000;
                        });

                        foreach (var descriptor in services
                                     .Where(d => d.ServiceType == typeof(IHostedService))
                                     .ToList())
                        {
                            services.Remove(descriptor);
                        }
                    })
                    .Configure(app =>
                    {
                        app.UseThrottleWatch();
                        app.Run(async context =>
                        {
                            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                            await context.Response.WriteAsync("too many");
                        });
                    });
            })
            .StartAsync();

        var client = host.GetTestClient();
        var response = await client.GetAsync("/api/limited");
        response.StatusCode.Should().Be(System.Net.HttpStatusCode.TooManyRequests);

        var buffer = host.Services.GetRequiredService<LocalMetricBuffer>();
        buffer.Count.Should().BeGreaterThan(0);

        var batch = buffer.DequeueBatch(1);
        batch.Should().ContainSingle();
        batch[0].Path.Should().Be("/api/limited");
        batch[0].StatusCode.Should().Be(429);

        await host.StopAsync();
    }
}
