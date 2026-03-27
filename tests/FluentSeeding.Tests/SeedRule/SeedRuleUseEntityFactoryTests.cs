using FluentAssertions;
using FluentSeeding.Tests.Common;

namespace FluentSeeding.Tests.SeedRule;

[TestFixture(TestName = "SeedRule.UseEntityFactory")]
[Category("Unit")]
[Category(nameof(SeedRule<,>))]
public sealed class SeedRuleUseEntityFactoryTests : SeedRuleTest
{
    #region Value production

    [Test]
    public void UseEntityFactory_WhenApplied_SetsPropertyFromEntity()
    {
        // Arrange
        var user = new User { Name = "Alice" };
        var rule = CreateRule(u => u.Email);
        rule.UseEntityFactory(u => u.Name + "@example.com");

        // Act
        rule.Apply(user);

        // Assert
        user.Email.Should().Be("Alice@example.com");
    }

    [Test]
    public void UseEntityFactory_WithIndex_PassesEntityAndIndex()
    {
        // Arrange
        var user = new User { Name = "Alice" };
        var rule = CreateRule(u => u.Email);
        rule.UseEntityFactory((u, i) => $"{u.Name}-{i}@example.com");

        // Act
        rule.Apply(user, 3);

        // Assert
        user.Email.Should().Be("Alice-3@example.com");
    }

    [Test]
    public void UseEntityFactory_WhenCalledMultipleTimes_UsesLastFactory()
    {
        // Arrange
        var user = new User { Name = "Alice" };
        var rule = CreateRule(u => u.Email);
        rule.UseEntityFactory(u => "first@example.com");
        rule.UseEntityFactory(u => u.Name + "@example.com");

        // Act
        rule.Apply(user);

        // Assert
        user.Email.Should().Be("Alice@example.com");
    }

    [Test]
    public void UseEntityFactory_AfterUseFactory_OverridesPreviousFactory()
    {
        // Arrange
        var user = new User { Name = "Alice" };
        var rule = CreateRule(u => u.Email);
        rule.UseFactory(() => "static@example.com");
        rule.UseEntityFactory(u => u.Name + "@example.com");

        // Act
        rule.Apply(user);

        // Assert
        user.Email.Should().Be("Alice@example.com");
    }

    [Test]
    public void UseFactory_AfterUseEntityFactory_OverridesPreviousFactory()
    {
        // Arrange
        var user = new User { Name = "Alice" };
        var rule = CreateRule(u => u.Email);
        rule.UseEntityFactory(u => u.Name + "@example.com");
        rule.UseFactory(() => "static@example.com");

        // Act
        rule.Apply(user);

        // Assert
        user.Email.Should().Be("static@example.com");
    }

    #endregion

    #region Automatic dependency detection

    [Test]
    public void UseEntityFactory_RegistersDirectMemberAccessesAsDependencies()
    {
        // Arrange
        var rule = CreateRule(u => u.Email);

        // Act
        rule.UseEntityFactory(u => u.Name + "@example.com");

        // Assert
        rule.Dependencies.Should().Contain(nameof(User.Name));
    }

    [Test]
    public void UseEntityFactory_WithMultipleMembers_RegistersAllAsDependencies()
    {
        // Arrange
        var rule = CreateRule(u => u.Email);

        // Act
        rule.UseEntityFactory(u => u.Name + u.Id.ToString());

        // Assert
        rule.Dependencies.Should().Contain(nameof(User.Name))
            .And.Contain(nameof(User.Id));
    }

    [Test]
    public void UseEntityFactory_WithIndex_RegistersDirectMemberAccessesAsDependencies()
    {
        // Arrange
        var rule = CreateRule(u => u.Email);

        // Act
        rule.UseEntityFactory((u, i) => $"{u.Name}-{i}@example.com");

        // Assert
        rule.Dependencies.Should().Contain(nameof(User.Name));
    }

    [Test]
    public void UseEntityFactory_DoesNotRegisterNonEntityExpressions_AsDependencies()
    {
        // Arrange
        var rule = CreateRule(u => u.Email);
        var prefix = "user";

        // Act
        rule.UseEntityFactory(u => prefix + "@example.com");

        // Assert
        rule.Dependencies.Should().BeEmpty();
    }

    [Test]
    public void UseEntityFactory_WithNestedAccess_RegistersRootMemberOnly()
    {
        // Arrange
        var rule = CreateRule(u => u.Name);

        // Act — u.Purchases.Count accesses Purchases directly on u, not Count
        rule.UseEntityFactory(u => $"user-{u.Purchases.Count}");

        // Assert
        rule.Dependencies.Should().ContainSingle().Which.Should().Be(nameof(User.Purchases));
    }

    [Test]
    public void UseEntityFactory_DoesNotOverwriteManualDependsOn()
    {
        // Arrange
        var rule = CreateRule(u => u.Email);
        rule.DependsOn(u => u.Id);

        // Act
        rule.UseEntityFactory(u => u.Name + "@example.com");

        // Assert
        rule.Dependencies.Should().Contain(nameof(User.Id))
            .And.Contain(nameof(User.Name));
    }

    #endregion
}
