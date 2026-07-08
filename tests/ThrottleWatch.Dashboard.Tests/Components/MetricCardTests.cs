using Bunit;
using ThrottleWatch.Dashboard.Components.Cards;
using Xunit;

namespace ThrottleWatch.Dashboard.Tests.Components;

public sealed class MetricCardTests : TestContext
{
    [Fact]
    public void MetricCard_Renders_LabelAndValue()
    {
        var cut = RenderComponent<MetricCard>(p => p
            .Add(c => c.Label, "Total Requests")
            .Add(c => c.Value, "1,234"));

        Assert.Contains("Total Requests", cut.Markup);
        Assert.Contains("1,234", cut.Markup);
    }

    [Fact]
    public void MetricCard_Renders_AccentClass()
    {
        var cut = RenderComponent<MetricCard>(p => p
            .Add(c => c.Label, "Blocked")
            .Add(c => c.AccentVariant, "red")
            .Add(c => c.Value, "42"));

        Assert.Contains("metric-card__accent--red", cut.Markup);
    }

    [Fact]
    public void MetricCard_ShowsSkeleton_WhenLoading()
    {
        var cut = RenderComponent<MetricCard>(p => p
            .Add(c => c.Label, "Loading metric")
            .Add(c => c.Value, "—")
            .Add(c => c.IsLoading, true));

        Assert.Contains("skeleton", cut.Markup);
    }

    [Fact]
    public void MetricCard_Renders_Subtitle_WhenProvided()
    {
        var cut = RenderComponent<MetricCard>(p => p
            .Add(c => c.Label, "Requests")
            .Add(c => c.Value, "100")
            .Add(c => c.Subtitle, "Since startup"));

        Assert.Contains("Since startup", cut.Markup);
    }

    [Theory]
    [InlineData("blue")]
    [InlineData("green")]
    [InlineData("purple")]
    [InlineData("orange")]
    public void MetricCard_Renders_AllValidAccentVariants(string variant)
    {
        var cut = RenderComponent<MetricCard>(p => p
            .Add(c => c.Label, "Test")
            .Add(c => c.Value, "0")
            .Add(c => c.AccentVariant, variant));

        Assert.Contains($"metric-card__accent--{variant}", cut.Markup);
    }

    [Fact]
    public void MetricCard_Renders_Icon_WhenProvided()
    {
        var cut = RenderComponent<MetricCard>(p => p
            .Add(c => c.Label, "Active")
            .Add(c => c.Value, "5")
            .Add(c => c.Icon, "◈"));

        Assert.Contains("metric-card__icon", cut.Markup);
        Assert.Contains("◈", cut.Markup);
    }
}
