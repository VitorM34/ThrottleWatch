using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using ThrottleWatch.Application.Services;
using ThrottleWatch.Domain.Entities;
using ThrottleWatch.Domain.Enums;
using ThrottleWatch.Domain.Exceptions;
using ThrottleWatch.Domain.Interfaces;

namespace ThrottleWatch.Application.Tests.Services;

public sealed class InsightServiceTests
{
    private readonly IInsightRepository _repository = Substitute.For<IInsightRepository>();
    private readonly ILogger<InsightService> _logger = Substitute.For<ILogger<InsightService>>();
    private readonly InsightService _sut;

    public InsightServiceTests()
    {
        _sut = new InsightService(_repository, _logger);
    }

    [Fact]
    public async Task GetActiveInsightsAsync_ShouldMapEntities()
    {
        var insight = Insight.Create(
            InsightType.PeakHours,
            "Peak",
            "Traffic spike",
            AlertSeverity.Info);

        _repository.GetActiveInsightsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<Insight> { insight });

        var result = await _sut.GetActiveInsightsAsync(CancellationToken.None);

        result.Should().ContainSingle();
        result[0].Title.Should().Be("Peak");
        result[0].Type.Should().Be(InsightType.PeakHours);
        result[0].IsDismissed.Should().BeFalse();
    }

    [Fact]
    public async Task DismissInsightAsync_WhenMissing_ShouldThrow()
    {
        var id = Guid.NewGuid();
        _repository.GetByIdAsync(id, Arg.Any<CancellationToken>()).Returns((Insight?)null);

        var act = () => _sut.DismissInsightAsync(id, CancellationToken.None);

        await act.Should().ThrowAsync<InsightNotFoundException>();
    }

    [Fact]
    public async Task DismissInsightAsync_WhenFound_ShouldDismissAndUpdate()
    {
        var insight = Insight.Create(
            InsightType.HighBlockRate,
            "High",
            "Blocked a lot",
            AlertSeverity.Warning);
        _repository.GetByIdAsync(insight.Id, Arg.Any<CancellationToken>()).Returns(insight);

        await _sut.DismissInsightAsync(insight.Id, CancellationToken.None);

        insight.IsDismissed.Should().BeTrue();
        await _repository.Received(1).UpdateAsync(insight, Arg.Any<CancellationToken>());
    }
}
