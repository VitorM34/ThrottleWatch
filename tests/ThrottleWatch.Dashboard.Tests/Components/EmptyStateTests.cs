using Bunit;
using ThrottleWatch.Dashboard.Components.Shared;
using Xunit;

namespace ThrottleWatch.Dashboard.Tests.Components;

public sealed class EmptyStateTests : TestContext
{
    [Fact]
    public void EmptyState_Renders_DefaultTitle()
    {
        var cut = RenderComponent<EmptyState>();

        Assert.Contains("No data available", cut.Markup);
    }

    [Fact]
    public void EmptyState_Renders_CustomTitle()
    {
        var cut = RenderComponent<EmptyState>(p => p
            .Add(c => c.Title, "No endpoints recorded"));

        Assert.Contains("No endpoints recorded", cut.Markup);
    }

    [Fact]
    public void EmptyState_Renders_Message_WhenProvided()
    {
        var cut = RenderComponent<EmptyState>(p => p
            .Add(c => c.Title, "Empty")
            .Add(c => c.Message, "Try refreshing the page"));

        Assert.Contains("Try refreshing the page", cut.Markup);
    }

    [Fact]
    public void EmptyState_Renders_CustomIcon()
    {
        var cut = RenderComponent<EmptyState>(p => p
            .Add(c => c.Icon, "⤢")
            .Add(c => c.Title, "No endpoints"));

        Assert.Contains("⤢", cut.Markup);
    }

    [Fact]
    public void EmptyState_Has_StatusRole()
    {
        var cut = RenderComponent<EmptyState>(p => p
            .Add(c => c.Title, "Empty state test"));

        Assert.Contains("role=\"status\"", cut.Markup);
    }
}
