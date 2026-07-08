using Bunit;
using ThrottleWatch.Dashboard.Components.Cards;
using Xunit;

namespace ThrottleWatch.Dashboard.Tests.Components;

public sealed class ContentCardTests : TestContext
{
    [Fact]
    public void ContentCard_Renders_Title()
    {
        var cut = RenderComponent<ContentCard>(p => p
            .Add(c => c.Title, "Top Endpoints")
            .AddChildContent("content"));

        Assert.Contains("Top Endpoints", cut.Markup);
        Assert.Contains("card__title", cut.Markup);
    }

    [Fact]
    public void ContentCard_Renders_Subtitle_WhenProvided()
    {
        var cut = RenderComponent<ContentCard>(p => p
            .Add(c => c.Title, "Metrics")
            .Add(c => c.Subtitle, "Last 60 minutes")
            .AddChildContent("content"));

        Assert.Contains("Last 60 minutes", cut.Markup);
        Assert.Contains("card__subtitle", cut.Markup);
    }

    [Fact]
    public void ContentCard_DoesNotRender_Header_WithoutTitle()
    {
        var cut = RenderComponent<ContentCard>(p => p
            .AddChildContent("Only body"));

        Assert.DoesNotContain("card__header", cut.Markup);
    }

    [Fact]
    public void ContentCard_Renders_ChildContent()
    {
        var cut = RenderComponent<ContentCard>(p => p
            .AddChildContent("<span class='test-content'>Test</span>"));

        Assert.Contains("test-content", cut.Markup);
        Assert.Contains("Test", cut.Markup);
    }

    [Fact]
    public void ContentCard_Renders_Footer_WhenProvided()
    {
        var cut = RenderComponent<ContentCard>(p => p
            .Add(c => c.Title, "Card")
            .AddChildContent("body")
            .Add(c => c.Footer, builder =>
            {
                builder.AddMarkupContent(0, "<span>Footer text</span>");
            }));

        Assert.Contains("card__footer", cut.Markup);
        Assert.Contains("Footer text", cut.Markup);
    }
}
