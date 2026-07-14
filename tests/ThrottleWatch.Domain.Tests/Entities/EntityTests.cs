using FluentAssertions;
using ThrottleWatch.Domain.Entities;

namespace ThrottleWatch.Domain.Tests.Entities;

// Concrete subclass used only inside this test file.
file sealed class ConcreteEntity : Entity
{
    public ConcreteEntity() : base() { }
}

public sealed class EntityTests
{
    [Fact]
    public void Constructor_ShouldGenerateNonEmptyId()
    {
        var entity = new ConcreteEntity();

        entity.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void Constructor_ShouldSetCreatedAtToUtcNow()
    {
        var before = DateTimeOffset.UtcNow;
        var entity = new ConcreteEntity();
        var after = DateTimeOffset.UtcNow;

        entity.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }

    [Fact]
    public void TwoEntities_WithSameId_ShouldBeEqual()
    {
        var id = Guid.NewGuid();

        var a = new ConcreteEntity();
        var b = new ConcreteEntity();

        // Manipulate Id via reflection to simulate same-identity scenario
        typeof(Entity)
            .GetProperty(nameof(Entity.Id))!
            .SetValue(a, id);

        typeof(Entity)
            .GetProperty(nameof(Entity.Id))!
            .SetValue(b, id);

        a.Should().Be(b);
        (a == b).Should().BeTrue();
        a.GetHashCode().Should().Be(b.GetHashCode());
    }

    [Fact]
    public void TwoEntities_WithDifferentIds_ShouldNotBeEqual()
    {
        var a = new ConcreteEntity();
        var b = new ConcreteEntity();

        a.Should().NotBe(b);
        (a != b).Should().BeTrue();
    }

    [Fact]
    public void Equals_WithNull_ShouldReturnFalse()
    {
        var entity = new ConcreteEntity();

        entity.Equals(null).Should().BeFalse();
    }

    [Fact]
    public void OperatorEquals_WithBothNull_ShouldReturnTrue()
    {
        ConcreteEntity? left = null;
        ConcreteEntity? right = null;

        (left == right).Should().BeTrue();
    }
}
