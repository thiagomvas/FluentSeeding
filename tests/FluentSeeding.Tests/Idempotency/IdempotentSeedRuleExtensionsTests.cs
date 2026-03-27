using FluentAssertions;
using FluentSeeding.Tests.Common;

namespace FluentSeeding.Tests.Idempotency;

[TestFixture(TestName = "IdempotentSeedRuleExtensions")]
[Category("Unit")]
[Category(nameof(IdempotentSeedRuleExtensions))]
public sealed class IdempotentSeedRuleExtensionsTests
{
    private sealed class LongEntity
    {
        public long Score { get; set; }
    }

    #region UseIdempotentGuid

    [Test]
    public void UseIdempotentGuid_ReturnsParentBuilder()
    {
        // Arrange
        var builder = new SeedBuilder<User>();
        var rule = builder.RuleFor(u => u.Id);

        // Act
        var result = rule.UseIdempotentGuid();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void UseIdempotentGuid_Factory_ProducesDeterministicValue()
    {
        // Arrange
        var user = new User();
        var rule = new SeedBuilder<User>().RuleFor(u => u.Id);
        rule.UseIdempotentGuid();

        // Act
        rule.Apply(user, 0);

        // Assert
        user.Id.Should().Be(Idempotent.Guid<User>(0));
    }

    [Test]
    public void UseIdempotentGuid_WithSeed_PassesSeedToFactory()
    {
        // Arrange
        var user = new User();
        var rule = new SeedBuilder<User>().RuleFor(u => u.Id);
        rule.UseIdempotentGuid("my-seed");

        // Act
        rule.Apply(user, 0);

        // Assert
        user.Id.Should().Be(Idempotent.Guid<User>(0, "my-seed"));
    }

    #endregion

    #region UseIdempotentInt

    [Test]
    public void UseIdempotentInt_ReturnsParentBuilder()
    {
        // Arrange
        var builder = new SeedBuilder<Purchase>();
        var rule = builder.RuleFor(p => p.Quantity);

        // Act
        var result = rule.UseIdempotentInt();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void UseIdempotentInt_Factory_ProducesDeterministicValue()
    {
        // Arrange
        var purchase = new Purchase();
        var rule = new SeedBuilder<Purchase>().RuleFor(p => p.Quantity);
        rule.UseIdempotentInt();

        // Act
        rule.Apply(purchase, 0);

        // Assert
        purchase.Quantity.Should().Be(Idempotent.Int<Purchase>(0));
    }

    [Test]
    public void UseIdempotentInt_WithSeed_PassesSeedToFactory()
    {
        // Arrange
        var purchase = new Purchase();
        var rule = new SeedBuilder<Purchase>().RuleFor(p => p.Quantity);
        rule.UseIdempotentInt("my-seed");

        // Act
        rule.Apply(purchase, 0);

        // Assert
        purchase.Quantity.Should().Be(Idempotent.Int<Purchase>(0, "my-seed"));
    }

    #endregion

    #region UseIdempotentLong

    [Test]
    public void UseIdempotentLong_ReturnsParentBuilder()
    {
        // Arrange
        var builder = new SeedBuilder<LongEntity>();
        var rule = builder.RuleFor(e => e.Score);

        // Act
        var result = rule.UseIdempotentLong();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void UseIdempotentLong_Factory_ProducesDeterministicValue()
    {
        // Arrange
        var entity = new LongEntity();
        var rule = new SeedBuilder<LongEntity>().RuleFor(e => e.Score);
        rule.UseIdempotentLong();

        // Act
        rule.Apply(entity, 0);

        // Assert
        entity.Score.Should().Be(Idempotent.Long<LongEntity>(0));
    }

    [Test]
    public void UseIdempotentLong_WithSeed_PassesSeedToFactory()
    {
        // Arrange
        var entity = new LongEntity();
        var rule = new SeedBuilder<LongEntity>().RuleFor(e => e.Score);
        rule.UseIdempotentLong("my-seed");

        // Act
        rule.Apply(entity, 0);

        // Assert
        entity.Score.Should().Be(Idempotent.Long<LongEntity>(0, "my-seed"));
    }

    #endregion

    #region UseIdempotentSlug

    [Test]
    public void UseIdempotentSlug_ReturnsParentBuilder()
    {
        // Arrange
        var builder = new SeedBuilder<User>();
        var rule = builder.RuleFor(u => u.Name);

        // Act
        var result = rule.UseIdempotentSlug();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void UseIdempotentSlug_Factory_ProducesDeterministicValue()
    {
        // Arrange
        var user = new User();
        var rule = new SeedBuilder<User>().RuleFor(u => u.Name);
        rule.UseIdempotentSlug();

        // Act
        rule.Apply(user, 0);

        // Assert
        user.Name.Should().Be(Idempotent.Slug<User>(0));
    }

    [Test]
    public void UseIdempotentSlug_WithPrefix_PassesPrefixToFactory()
    {
        // Arrange
        var user = new User();
        var rule = new SeedBuilder<User>().RuleFor(u => u.Name);
        rule.UseIdempotentSlug("my-prefix");

        // Act
        rule.Apply(user, 0);

        // Assert
        user.Name.Should().Be("my-prefix-0");
    }

    #endregion
}
