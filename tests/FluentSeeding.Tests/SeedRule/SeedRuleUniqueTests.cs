using FluentAssertions;
using FluentSeeding.Tests.Common;

namespace FluentSeeding.Tests.SeedRule;

[TestFixture(TestName = "SeedRule.Unique")]
[Category("Unit")]
[Category(nameof(SeedRule<,>))]
public sealed class SeedRuleUniqueTests : SeedRuleTest
{
    [Test]
    public void Unique_WhenNoRepeats_ShouldNotThrow()
    {
        // Arrange
        var rule = CreateRule(u => u.Name);
        rule.Unique().UseFactory(i => i.ToString());
        var user1 = new User { Name = "Test Name" };
        var user2 = new User { Name = "Another Name" };
        
        // Act & Assert
        rule.Unique().Apply(user1, 0);
        rule.Unique().Apply(user2, 1); 
    }

    [Test]
    public void Unique_WithRepeats_ShouldThrow()
    {
        // Arrange
        var rule = CreateRule(u => u.Name);
        rule.Unique().UseValue("Same Name");
        var user1 = new User { Name = "Test Name" };
        var user2 = new User { Name = "Another Name" };
        
        // Act
        Action act1 = () => rule.Unique().Apply(user1);
        Action act2 = () => rule.Unique().Apply(user2); 
        
        // Assert
        act1.Should().NotThrow();
        act2.Should().Throw<InvalidOperationException>();
        
    }
    
}
