namespace ThrottleWatch.Application.Interfaces;

public interface IOperationalMetrics
{
    void RecordBatchProcessed(int count, double durationMs);

    void RecordDrop();
}
