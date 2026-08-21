using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace ThrottleWatch.Client.Metrics;

public sealed class ClientMetrics
{
    public const string MeterName = "ThrottleWatch.Client";
    public const string MeterVersion = "1.0.0";

    public const string RequestsInstrument = "throttlewatch.client.requests";
    public const string DroppedInstrument = "throttlewatch.client.buffer.dropped";
    public const string FlushInstrument = "throttlewatch.client.flush.metrics";
    public const string FlushErrorsInstrument = "throttlewatch.client.flush.errors";

    public const string MethodTag = "http.method";
    public const string BlockedTag = "blocked";
    public const string PolicyTag = "throttlewatch.policy";

    private readonly Counter<long> _requests;
    private readonly Counter<long> _dropped;
    private readonly Counter<long> _flushed;
    private readonly Counter<long> _flushErrors;

    public ClientMetrics(IMeterFactory meterFactory)
    {
        ArgumentNullException.ThrowIfNull(meterFactory);

        var meter = meterFactory.Create(MeterName, MeterVersion);

        _requests = meter.CreateCounter<long>(
            RequestsInstrument,
            unit: "{requests}",
            description: "HTTP requests observed by ThrottleWatch.Client (independent of ingest buffer).");

        _dropped = meter.CreateCounter<long>(
            DroppedInstrument,
            unit: "{requests}",
            description: "Requests dropped because the local ingest buffer was full.");

        _flushed = meter.CreateCounter<long>(
            FlushInstrument,
            unit: "{requests}",
            description: "Metric payloads successfully posted to ThrottleWatch.Api.");

        _flushErrors = meter.CreateCounter<long>(
            FlushErrorsInstrument,
            unit: "{batches}",
            description: "Failed HTTP flushes to ThrottleWatch.Api.");
    }

    public void RecordRequest(string method, bool blocked, string? policyName)
    {
        var tags = new TagList
        {
            { MethodTag, string.IsNullOrWhiteSpace(method) ? "UNKNOWN" : method },
            { BlockedTag, blocked ? "true" : "false" }
        };

        if (!string.IsNullOrWhiteSpace(policyName))
            tags.Add(PolicyTag, policyName);

        _requests.Add(1, tags);
    }

    public void RecordDrop() => _dropped.Add(1);

    public void RecordFlush(int count)
    {
        if (count > 0)
            _flushed.Add(count);
    }

    public void RecordFlushError() => _flushErrors.Add(1);
}
