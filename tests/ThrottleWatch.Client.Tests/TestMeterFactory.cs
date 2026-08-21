using System.Diagnostics.Metrics;

namespace ThrottleWatch.Client.Tests;

internal sealed class TestMeterFactory : IMeterFactory
{
    public Meter Create(MeterOptions options) => new(options);

    public void Dispose()
    {
    }
}
