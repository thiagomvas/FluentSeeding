using FluentAssertions;
using FluentSeeding.Tests.Common;

namespace FluentSeeding.Tests.SeedRule;

[TestFixture(TestName = "SeedRule.UseFactory")]
[Category("Unit")]
[Category(nameof(SeedRule<,>))]
public sealed class SeedRuleUseFactoryTests : SeedRuleTest
{
    [Test]
    public void UseFactory_WhenCalled_ReturnsSameRuleForChaining()
    {
        // Arrange
        var rule = CreateRule(u => u.Name);

        // Act
        var returned = rule.UseFactory(() => "Test");

        // Assert
        returned.Should().BeSameAs(rule);
    }

    [Test]
    public void UseFactory_WhenApplied_SetsPropertyToFactoryResult()
    {
        // Arrange
        var rule = CreateRule(u => u.Name);
        rule.UseFactory(() => "Generated");
        var user = new User();

        // Act
        rule.Apply(user);

        // Assert
        user.Name.Should().Be("Generated");
    }

    [Test]
    public void UseFactory_WhenAppliedMultipleTimes_InvokesFactoryEachTime()
    {
        // Arrange
        var callCount = 0;
        var rule = CreateRule(u => u.Name);
        rule.UseFactory(() =>
        {
            callCount++;
            return $"Call{callCount}";
        });

        var user1 = new User();
        var user2 = new User();

        // Act
        rule.Apply(user1);
        rule.Apply(user2);

        // Assert
        callCount.Should().Be(2);
        user1.Name.Should().Be("Call1");
        user2.Name.Should().Be("Call2");
    }

    [Test]
    public void UseFactory_WhenCalledMultipleTimes_UsesLastFactory()
    {
        // Arrange
        var rule = CreateRule(u => u.Name);
        rule.UseFactory(() => "First");
        rule.UseFactory(() => "Second");
        var user = new User();

        // Act
        rule.Apply(user);

        // Assert
        user.Name.Should().Be("Second");
    }

    [Test]
    public void UseFactory_WithValueTypeProperty_SetsPropertyCorrectly()
    {
        // Arrange
        var expectedId = Guid.NewGuid();
        var rule = CreateRule(u => u.Id);
        rule.UseFactory(() => expectedId);
        var user = new User();

        // Act
        rule.Apply(user);

        // Assert
        user.Id.Should().Be(expectedId);
    }
}
