namespace ThrottleWatch.Dashboard.Services;

public sealed class ThemeService : IThemeService
{
    private bool _isDarkMode = true;

    public bool IsDarkMode => _isDarkMode;

    public event EventHandler? ThemeChanged;

    public void ToggleTheme()
    {
        _isDarkMode = !_isDarkMode;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }

    public void SetDarkMode(bool isDark)
    {
        if (_isDarkMode == isDark) return;
        _isDarkMode = isDark;
        ThemeChanged?.Invoke(this, EventArgs.Empty);
    }
}
