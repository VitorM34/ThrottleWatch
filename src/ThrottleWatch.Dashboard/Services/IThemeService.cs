namespace ThrottleWatch.Dashboard.Services;

public interface IThemeService
{
    bool IsDarkMode { get; }
    event EventHandler? ThemeChanged;
    void ToggleTheme();
    void SetDarkMode(bool isDark);
}
