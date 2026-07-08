using ThrottleWatch.Dashboard.Services;
using Xunit;

namespace ThrottleWatch.Dashboard.Tests.Services;

public sealed class ToastServiceTests
{
    [Fact]
    public void ShowSuccess_RaisesToastAdded_WithSuccessType()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.ToastAdded += (_, msg) => received = msg;

        service.ShowSuccess("Operation completed");

        Assert.NotNull(received);
        Assert.Equal(ToastType.Success, received.Type);
        Assert.Equal("Operation completed", received.Message);
    }

    [Fact]
    public void ShowError_RaisesToastAdded_WithErrorType()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.ToastAdded += (_, msg) => received = msg;

        service.ShowError("Something failed");

        Assert.NotNull(received);
        Assert.Equal(ToastType.Error, received.Type);
    }

    [Fact]
    public void ShowWarning_RaisesToastAdded_WithWarningType()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.ToastAdded += (_, msg) => received = msg;

        service.ShowWarning("Be careful");

        Assert.NotNull(received);
        Assert.Equal(ToastType.Warning, received.Type);
    }

    [Fact]
    public void ShowInfo_RaisesToastAdded_WithInfoType()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.ToastAdded += (_, msg) => received = msg;

        service.ShowInfo("Just so you know");

        Assert.NotNull(received);
        Assert.Equal(ToastType.Info, received.Type);
    }

    [Fact]
    public void ShowSuccess_Sets_TitleWhenProvided()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.ToastAdded += (_, msg) => received = msg;

        service.ShowSuccess("Done", "Success");

        Assert.Equal("Success", received?.Title);
    }

    [Fact]
    public void Dismiss_RaisesToastRemoved_WithCorrectId()
    {
        var service = new ToastService();
        ToastMessage? added = null;
        Guid? removedId = null;

        service.ToastAdded   += (_, msg) => added      = msg;
        service.ToastRemoved += (_, id)  => removedId  = id;

        service.ShowSuccess("Test");
        service.Dismiss(added!.Id);

        Assert.Equal(added.Id, removedId);
    }

    [Fact]
    public void ShowError_UsesLongerDuration()
    {
        var service = new ToastService();
        ToastMessage? received = null;
        service.ToastAdded += (_, msg) => received = msg;

        service.ShowSuccess("ok");
        var successDuration = received!.DurationMs;

        service.ShowError("error");
        var errorDuration = received.DurationMs;

        Assert.True(errorDuration > successDuration);
    }

    [Fact]
    public void ToastMessage_HasUniqueIds()
    {
        var service = new ToastService();
        var ids = new List<Guid>();
        service.ToastAdded += (_, msg) => ids.Add(msg.Id);

        service.ShowSuccess("First");
        service.ShowSuccess("Second");
        service.ShowSuccess("Third");

        Assert.Equal(3, ids.Distinct().Count());
    }
}
