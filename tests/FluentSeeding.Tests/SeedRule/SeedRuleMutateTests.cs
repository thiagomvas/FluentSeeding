using FluentAssertions;
using FluentSeeding.Tests.Common;

namespace FluentSeeding.Tests.SeedRule;

[TestFixture(TestName = "SeedRule.Mutate")]
[Category("Unit")]
[Category(nameof(SeedRule<,>))]
public sealed class SeedRuleMutateTests : SeedRuleTest
{
    [Test]
    public void Mutate_TransformsGeneratedValue()
    {
        // Arrange
        var rule = CreateRule(u => u.Name);
        rule.Mutate(v => v.ToUpperInvariant()).UseValue("hello");
        var user = new User();

        // Act
        rule.Apply(user);

        // Assert
        user.Name.Should().Be("HELLO");
    }

    [Test]
    public void Mutate_ReceivesFactoryOutput()
    {
        // Arrange
        var rule = CreateRule(u => u.Name);
        rule.Mutate(v => $"[{v}]").UseFactory(i => $"item{i}");
        var user = new User();

        // Act
        rule.Apply(user, 3);

        // Assert
        user.Name.Should().Be("[item3]");
    }

    [Test]
    public void Mutate_IsAppliedAfterUniqueCheck()
    {
        // Arrange — factory produces unique values; mutator collapses them to the same string,
        // but uniqueness must still be satisfied on the raw value, not the mutated one.
        var rule = CreateRule(u => u.Name);
        rule.Unique().Mutate(_ => "same").UseFactory(i => i.ToString());
        var user1 = new User();
        var user2 = new User();

        // Act
        rule.Apply(user1, 0);
        rule.Apply(user2, 1);

        // Assert — both entities end up with the mutated value without a uniqueness exception
        user1.Name.Should().Be("same");
        user2.Name.Should().Be("same");
    }

    [Test]
    public void Mutate_WithoutMutator_ValuePassesThroughUnchanged()
    {
        // Arrange
        var rule = CreateRule(u => u.Name);
        rule.UseValue("original");
        var user = new User();

        // Act
        rule.Apply(user);

        // Assert
        user.Name.Should().Be("original");
    }

    [Test]
    public void Mutate_ReturnsRuleForChaining()
    {
        // Arrange
        var rule = CreateRule(u => u.Name);

        // Act
        var returned = rule.Mutate(v => v);

        // Assert
        returned.Should().BeSameAs(rule);
    }
}
