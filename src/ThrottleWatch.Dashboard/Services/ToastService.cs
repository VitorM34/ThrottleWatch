namespace ThrottleWatch.Dashboard.Services;

public sealed class ToastService : IToastService
{
    public event EventHandler<ToastMessage>? ToastAdded;
    public event EventHandler<Guid>? ToastRemoved;

    public void ShowSuccess(string message, string? title = null) =>
        Add(message, title, ToastType.Success);

    public void ShowError(string message, string? title = null) =>
        Add(message, title, ToastType.Error, durationMs: 6000);

    public void ShowWarning(string message, string? title = null) =>
        Add(message, title, ToastType.Warning);

    public void ShowInfo(string message, string? title = null) =>
        Add(message, title, ToastType.Info);

    public void Dismiss(Guid id) =>
        ToastRemoved?.Invoke(this, id);

    private void Add(string message, string? title, ToastType type, int durationMs = 4000)
    {
        var toast = new ToastMessage
        {
            Message = message,
            Title = title,
            Type = type,
            DurationMs = durationMs
        };
        ToastAdded?.Invoke(this, toast);
    }
}
