using ThrottleWatch.Dashboard;

namespace ThrottleWatch.Dashboard.Tests;

public sealed class DashboardPathTests
{
    [Theory]
    [InlineData(null, "/")]
    [InlineData("", "/")]
    [InlineData("/", "/")]
    [InlineData("throttlewatch", "/throttlewatch")]
    [InlineData("/throttlewatch", "/throttlewatch")]
    [InlineData("/throttlewatch/", "/throttlewatch")]
    public void NormalizePrefix_ShouldMatchMicrosoftPathBaseShape(string? path, string expected)
    {
        Assert.Equal(expected, DashboardPath.NormalizePrefix(path));
    }

    [Theory]
    [InlineData(null, "/")]
    [InlineData("", "/")]
    [InlineData("/", "/")]
    [InlineData("/throttlewatch", "/throttlewatch/")]
    [InlineData("/throttlewatch/", "/throttlewatch/")]
    public void ToBaseHref_ShouldRequireTrailingSlashWhenPrefixed(string? pathBase, string expected)
    {
        Assert.Equal(expected, DashboardPath.ToBaseHref(pathBase));
    }

    [Theory]
    [InlineData(null, "/_content/ThrottleWatch.Dashboard/icons/users.svg")]
    [InlineData("/", "/_content/ThrottleWatch.Dashboard/icons/users.svg")]
    [InlineData("/throttlewatch", "/throttlewatch/_content/ThrottleWatch.Dashboard/icons/users.svg")]
    [InlineData("/throttlewatch/", "/throttlewatch/_content/ThrottleWatch.Dashboard/icons/users.svg")]
    public void IconHref_ShouldBeRootAbsoluteForCssMask(string? pathBase, string expected)
    {
        Assert.Equal(expected, DashboardPath.IconHref(pathBase, "users"));
    }
}
