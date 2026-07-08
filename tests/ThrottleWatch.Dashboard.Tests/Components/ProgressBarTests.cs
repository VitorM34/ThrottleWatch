using Bunit;
using ThrottleWatch.Dashboard.Components.Shared;
using Xunit;

namespace ThrottleWatch.Dashboard.Tests.Components;

public sealed class ProgressBarTests : TestContext
{
    [Fact]
    public void ProgressBar_Renders_WithValue()
    {
        var cut = RenderComponent<ProgressBar>(p => p
            .Add(c => c.Value, 50));

        Assert.Contains("width: 50%", cut.Markup);
    }

    [Fact]
    public void ProgressBar_ClampsValue_AboveHundred()
    {
        var cut = RenderComponent<ProgressBar>(p => p
            .Add(c => c.Value, 150));

        Assert.Contains("width: 100%", cut.Markup);
        Assert.DoesNotContain("width: 150%", cut.Markup);
    }

    [Fact]
    public void ProgressBar_ClampsValue_BelowZero()
    {
        var cut = RenderComponent<ProgressBar>(p => p
            .Add(c => c.Value, -10));

        Assert.Contains("width: 0%", cut.Markup);
    }

    [Theory]
    [InlineData("blue")]
    [InlineData("green")]
    [InlineData("red")]
    [InlineData("orange")]
    public void ProgressBar_Renders_CorrectVariantClass(string variant)
    {
        var cut = RenderComponent<ProgressBar>(p => p
            .Add(c => c.Value, 30)
            .Add(c => c.Variant, variant));

        Assert.Contains($"progress__bar--{variant}", cut.Markup);
    }

    [Fact]
    public void ProgressBar_Renders_AriaAttributes()
    {
        var cut = RenderComponent<ProgressBar>(p => p
            .Add(c => c.Value, 75)
            .Add(c => c.Label, "Request volume"));

        Assert.Contains("role=\"progressbar\"", cut.Markup);
        Assert.Contains("aria-valuenow=\"75\"", cut.Markup);
        Assert.Contains("aria-valuemin=\"0\"", cut.Markup);
        Assert.Contains("aria-valuemax=\"100\"", cut.Markup);
    }
}
