using System.Diagnostics.Metrics;
using FluentAssertions;
using ThrottleWatch.Client.Metrics;

namespace ThrottleWatch.Client.Tests.Metrics;

public sealed class ClientMetricsTests : IDisposable
{
    private readonly MeterListener _listener = new();
    private readonly List<(string Instrument, long Value, string? Method, string? Blocked, string? Policy)> _measurements = [];

    public ClientMetricsTests()
    {
        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (instrument.Meter.Name == ClientMetrics.MeterName)
                listener.EnableMeasurementEvents(instrument);
        };

        _listener.SetMeasurementEventCallback<long>((instrument, value, tags, _) =>
        {
            lock (_measurements)
            {
                _measurements.Add((
                    instrument.Name,
                    value,
                    GetTag(tags, ClientMetrics.MethodTag),
                    GetTag(tags, ClientMetrics.BlockedTag),
                    GetTag(tags, ClientMetrics.PolicyTag)));
            }
        });

        _listener.Start();
    }

    [Fact]
    public void RecordRequest_ShouldTagBlockedVsAllowed()
    {
        var metrics = new ClientMetrics(new TestMeterFactory());

        metrics.RecordRequest("GET", blocked: false, policyName: "fixed-window");
        metrics.RecordRequest("POST", blocked: true, policyName: null);

        Snapshot(ClientMetrics.RequestsInstrument).Should().HaveCount(2);
        Snapshot(ClientMetrics.RequestsInstrument).Should().Contain(m =>
            m.Method == "GET" && m.Blocked == "false" && m.Policy == "fixed-window");
        Snapshot(ClientMetrics.RequestsInstrument).Should().Contain(m =>
            m.Method == "POST" && m.Blocked == "true" && m.Policy == null);
    }

    [Fact]
    public void RecordDrop_ShouldIncrementDroppedCounter()
    {
        var metrics = new ClientMetrics(new TestMeterFactory());

        metrics.RecordDrop();

        Snapshot(ClientMetrics.DroppedInstrument).Should().ContainSingle().Which.Value.Should().Be(1);
    }

    [Fact]
    public void RecordFlush_ShouldAddPayloadCount_AndIgnoreZero()
    {
        var metrics = new ClientMetrics(new TestMeterFactory());

        metrics.RecordFlush(3);
        metrics.RecordFlush(0);

        Snapshot(ClientMetrics.FlushInstrument).Should().ContainSingle().Which.Value.Should().Be(3);
    }

    [Fact]
    public void RecordFlushError_ShouldIncrementErrorCounter()
    {
        var metrics = new ClientMetrics(new TestMeterFactory());

        metrics.RecordFlushError();

        Snapshot(ClientMetrics.FlushErrorsInstrument).Should().ContainSingle().Which.Value.Should().Be(1);
    }

    public void Dispose() => _listener.Dispose();

    private List<(string Instrument, long Value, string? Method, string? Blocked, string? Policy)> Snapshot(
        string instrument)
    {
        lock (_measurements)
            return _measurements.Where(m => m.Instrument == instrument).ToList();
    }

    private static string? GetTag(ReadOnlySpan<KeyValuePair<string, object?>> tags, string name)
    {
        foreach (var tag in tags)
        {
            if (tag.Key == name)
                return tag.Value?.ToString();
        }

        return null;
    }
}
