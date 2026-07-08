using ThrottleWatch.Dashboard.Services;
using Xunit;

namespace ThrottleWatch.Dashboard.Tests.Services;

public sealed class ThemeServiceTests
{
    [Fact]
    public void ThemeService_DefaultsTo_DarkMode()
    {
        var service = new ThemeService();

        Assert.True(service.IsDarkMode);
    }

    [Fact]
    public void ThemeService_ToggleTheme_SwitchesToLight()
    {
        var service = new ThemeService();

        service.ToggleTheme();

        Assert.False(service.IsDarkMode);
    }

    [Fact]
    public void ThemeService_ToggleTheme_SwitchesBackToDark()
    {
        var service = new ThemeService();

        service.ToggleTheme();
        service.ToggleTheme();

        Assert.True(service.IsDarkMode);
    }

    [Fact]
    public void ThemeService_SetDarkMode_RaisesEvent_WhenValueChanges()
    {
        var service = new ThemeService();
        var eventRaised = false;
        service.ThemeChanged += (_, _) => eventRaised = true;

        service.SetDarkMode(false);

        Assert.True(eventRaised);
    }

    [Fact]
    public void ThemeService_SetDarkMode_DoesNotRaise_WhenValueUnchanged()
    {
        var service = new ThemeService();
        var eventCount = 0;
        service.ThemeChanged += (_, _) => eventCount++;

        service.SetDarkMode(true);

        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void ThemeService_ToggleTheme_RaisesChangedEvent()
    {
        var service = new ThemeService();
        var eventRaised = false;
        service.ThemeChanged += (_, _) => eventRaised = true;

        service.ToggleTheme();

        Assert.True(eventRaised);
    }
}
