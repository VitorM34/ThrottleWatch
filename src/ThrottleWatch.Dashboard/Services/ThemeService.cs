using Microsoft.JSInterop;

namespace ThrottleWatch.Dashboard.Services;

public sealed class ThemeService : IThemeService
{
    private readonly IJSRuntime _js;
    private bool _isDarkMode = true;

    public ThemeService(IJSRuntime js) => _js = js;

    public bool IsDarkMode => _isDarkMode;

    public event EventHandler? ThemeChanged;

    public void ToggleTheme() => SetDarkMode(!_isDarkMode);

    public void SetDarkMode(bool isDark)
    {
        if (_isDarkMode == isDark)
        {
            _ = PersistAsync();
            return;
        }

        _isDarkMode = isDark;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
        _ = PersistAsync();
    }

    private async Task PersistAsync()
    {
        try
        {
            await _js.InvokeVoidAsync("twTheme.set", _isDarkMode ? "dark" : "light");
        }
        catch (JSException)
        {
            // Circuit may be disposing; ignore.
        }
        catch (InvalidOperationException)
        {
            // JS runtime unavailable during early circuit lifecycle.
        }
    }
}
