using Bunit;
using ThrottleWatch.Dashboard.Components.Shared;
using Xunit;

namespace ThrottleWatch.Dashboard.Tests.Components;

public sealed class AlertBannerTests : TestContext
{
    [Fact]
    public void AlertBanner_Renders_ByDefault()
    {
        var cut = RenderComponent<AlertBanner>(p => p
            .Add(c => c.Message, "Something happened"));

        Assert.Contains("Something happened", cut.Markup);
        Assert.Contains("alert--info", cut.Markup);
    }

    [Theory]
    [InlineData("success")]
    [InlineData("warning")]
    [InlineData("error")]
    [InlineData("info")]
    public void AlertBanner_Renders_WithVariant(string variant)
    {
        var cut = RenderComponent<AlertBanner>(p => p
            .Add(c => c.Variant, variant)
            .Add(c => c.Message, "Alert message"));

        Assert.Contains($"alert--{variant}", cut.Markup);
    }

    [Fact]
    public void AlertBanner_Renders_Title_WhenProvided()
    {
        var cut = RenderComponent<AlertBanner>(p => p
            .Add(c => c.Title, "Error Title")
            .Add(c => c.Message, "Error message"));

        Assert.Contains("Error Title", cut.Markup);
    }

    [Fact]
    public void AlertBanner_IsHidden_AfterDismiss()
    {
        var cut = RenderComponent<AlertBanner>(p => p
            .Add(c => c.Dismissible, true)
            .Add(c => c.Message, "Dismissible alert"));

        var dismissBtn = cut.Find("button[aria-label='Dismiss alert']");
        dismissBtn.Click();

        Assert.Empty(cut.Markup.Trim());
    }

    [Fact]
    public void AlertBanner_DoesNotRender_DismissButton_ByDefault()
    {
        var cut = RenderComponent<AlertBanner>(p => p
            .Add(c => c.Message, "Non dismissible"));

        Assert.DoesNotContain("Dismiss alert", cut.Markup);
    }

    [Fact]
    public void AlertBanner_Has_AlertRole()
    {
        var cut = RenderComponent<AlertBanner>(p => p
            .Add(c => c.Message, "Accessible alert"));

        Assert.Contains("role=\"alert\"", cut.Markup);
    }
}
