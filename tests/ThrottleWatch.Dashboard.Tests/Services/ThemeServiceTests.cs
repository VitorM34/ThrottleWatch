using Microsoft.JSInterop;
using NSubstitute;
using ThrottleWatch.Dashboard.Services;
using Xunit;

namespace ThrottleWatch.Dashboard.Tests.Services;

public sealed class ThemeServiceTests
{
    private static ThemeService CreateSut() => new(Substitute.For<IJSRuntime>());

    [Fact]
    public void ThemeService_DefaultsTo_DarkMode()
    {
        var service = CreateSut();

        Assert.True(service.IsDarkMode);
    }

    [Fact]
    public void ThemeService_ToggleTheme_SwitchesToLight()
    {
        var service = CreateSut();

        service.ToggleTheme();

        Assert.False(service.IsDarkMode);
    }

    [Fact]
    public void ThemeService_ToggleTheme_SwitchesBackToDark()
    {
        var service = CreateSut();

        service.ToggleTheme();
        service.ToggleTheme();

        Assert.True(service.IsDarkMode);
    }

    [Fact]
    public void ThemeService_SetDarkMode_RaisesEvent_WhenValueChanges()
    {
        var service = CreateSut();
        var eventRaised = false;
        service.ThemeChanged += (_, _) => eventRaised = true;

        service.SetDarkMode(false);

        Assert.True(eventRaised);
    }

    [Fact]
    public void ThemeService_SetDarkMode_DoesNotRaise_WhenValueUnchanged()
    {
        var service = CreateSut();
        var eventCount = 0;
        service.ThemeChanged += (_, _) => eventCount++;

        service.SetDarkMode(true);

        Assert.Equal(0, eventCount);
    }

    [Fact]
    public void ThemeService_ToggleTheme_RaisesChangedEvent()
    {
        var service = CreateSut();
        var eventRaised = false;
        service.ThemeChanged += (_, _) => eventRaised = true;

        service.ToggleTheme();

        Assert.True(eventRaised);
    }
}
