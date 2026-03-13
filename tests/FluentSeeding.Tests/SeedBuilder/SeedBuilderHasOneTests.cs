using FluentAssertions;
using FluentSeeding.Tests.Common;

namespace FluentSeeding.Tests.SeedBuilder;

[TestFixture(TestName = "SeedBuilder.HasOne")]
[Category("Unit")]
[Category(nameof(SeedBuilder<>))]
public sealed class SeedBuilderHasOneTests
{
    [Test]
    public void HasOne_WhenBuilt_PopulatesNestedProperty()
    {
        // Arrange
        var builder = new SeedBuilder<Profile>();
        builder.HasOne(p => p.User, u => u
            .RuleFor(user => user.Name).UseValue("Alice"));

        // Act
        var profile = builder.Build().Single();

        // Assert
        profile.User.Should().NotBeNull();
    }

    [Test]
    public void HasOne_WhenBuilt_AppliesConfiguredPropertiesToNestedObject()
    {
        // Arrange
        var builder = new SeedBuilder<Profile>();
        builder.HasOne(p => p.User, u => u
            .RuleFor(user => user.Name).UseValue("Alice")
            .RuleFor(user => user.Email).UseValue("alice@example.com"));

        // Act
        var profile = builder.Build().Single();

        // Assert
        profile.User.Name.Should().Be("Alice");
        profile.User.Email.Should().Be("alice@example.com");
    }

    [Test]
    public void HasOne_EachParentEntityReceivesItsOwnNestedInstance()
    {
        // Arrange
        var builder = new SeedBuilder<Profile>();
        builder
            .Count(2)
            .HasOne(p => p.User, u => u
                .RuleFor(user => user.Name).UseValue("Bob"));

        // Act
        var profiles = builder.Build().ToList();

        // Assert
        profiles.Should().HaveCount(2);
        profiles[0].User.Should().NotBeSameAs(profiles[1].User);
    }

    [Test]
    public void HasOne_CountInConfigure_IsIgnored_OnlyOneNestedInstanceIsProduced()
    {
        // Arrange
        var builder = new SeedBuilder<Profile>();
        builder.HasOne(p => p.User, u => u
            .Count(5) // HasOne overrides this to 1; without the override, .Single() inside HasOne would throw
            .RuleFor(user => user.Name).UseValue("Charlie"));

        // Act
        var profile = builder.Build().Single();

        // Assert
        profile.User.Should().NotBeNull();
        profile.User.Name.Should().Be("Charlie");
    }

    [Test]
    public void HasOne_WithNestedSelector_ThrowsArgumentException()
    {
        // Arrange
        var builder = new SeedBuilder<Profile>();

        // Act
        Action act = () => builder.HasOne(p => p.User.Email, b => { });

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithParameterName("selector");
    }
}
