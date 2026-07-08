namespace ThrottleWatch.Dashboard.Services;

public interface IToastService
{
    event EventHandler<ToastMessage>? ToastAdded;
    event EventHandler<Guid>? ToastRemoved;

    void ShowSuccess(string message, string? title = null);
    void ShowError(string message, string? title = null);
    void ShowWarning(string message, string? title = null);
    void ShowInfo(string message, string? title = null);
    void Dismiss(Guid id);
}

public sealed class ToastMessage
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Message { get; set; } = string.Empty;
    public string? Title { get; set; }
    public ToastType Type { get; set; }
    public int DurationMs { get; set; } = 4000;
    public DateTimeOffset CreatedAt { get; } = DateTimeOffset.UtcNow;
}

public enum ToastType
{
    Success,
    Error,
    Warning,
    Info
}
