using Bunit;
using ThrottleWatch.Dashboard.Components.Shared;
using Xunit;

namespace ThrottleWatch.Dashboard.Tests.Components;

public sealed class BadgeTests : TestContext
{
    [Fact]
    public void Badge_Renders_WithDefaultVariant()
    {
        var cut = RenderComponent<Badge>(p => p
            .AddChildContent("Hello"));

        Assert.Contains("badge--neutral", cut.Markup);
        Assert.Contains("Hello", cut.Markup);
    }

    [Theory]
    [InlineData("success")]
    [InlineData("warning")]
    [InlineData("error")]
    [InlineData("info")]
    [InlineData("purple")]
    public void Badge_Renders_WithVariant(string variant)
    {
        var cut = RenderComponent<Badge>(p => p
            .Add(b => b.Variant, variant)
            .AddChildContent(variant));

        Assert.Contains($"badge--{variant}", cut.Markup);
    }

    [Fact]
    public void Badge_Renders_DotWhenShowDotIsTrue()
    {
        var cut = RenderComponent<Badge>(p => p
            .Add(b => b.ShowDot, true)
            .AddChildContent("Dot"));

        Assert.Contains("badge__dot", cut.Markup);
    }

    [Fact]
    public void Badge_DoesNotRender_DotByDefault()
    {
        var cut = RenderComponent<Badge>(p => p
            .AddChildContent("No dot"));

        Assert.DoesNotContain("badge__dot", cut.Markup);
    }

    [Fact]
    public void Badge_Renders_PulseClass_WhenEnabled()
    {
        var cut = RenderComponent<Badge>(p => p
            .Add(b => b.Pulse, true)
            .Add(b => b.ShowDot, true)
            .AddChildContent("Pulse"));

        Assert.Contains("badge--pulse", cut.Markup);
    }

    [Fact]
    public void Badge_Renders_AriaLabel_WhenProvided()
    {
        var cut = RenderComponent<Badge>(p => p
            .Add(b => b.AriaLabel, "Status: active")
            .AddChildContent("Active"));

        Assert.Contains("aria-label=\"Status: active\"", cut.Markup);
    }
}
