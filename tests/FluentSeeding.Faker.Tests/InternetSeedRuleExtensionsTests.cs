using FluentAssertions;
using FluentSeeding.Faker.Extensions;
using FluentSeeding.Tests.Common;

namespace FluentSeeding.Faker.Tests;

[TestFixture(TestName = "InternetSeedRuleExtensions")]
[Category("Unit")]
[Category(nameof(InternetSeedRuleExtensions))]
public sealed class InternetSeedRuleExtensionsTests
{
    private static SeedRule<User, string> CreateEmailRule()
        => new SeedBuilder<User>().RuleFor(u => u.Email);

    private static string ApplyOnce(SeedRule<User, string> rule, User? user = null)
    {
        user ??= new User();
        rule.Apply(user);
        return user.Email;
    }

    #region Sanitize

    [TestCase("Alice", "alice")]
    [TestCase("JOHN", "john")]
    [TestCase("João", "joao")]
    [TestCase("García", "garcia")]
    [TestCase("André", "andre")]
    [TestCase("Thiago", "thiago")]
    public void Sanitize_RemovesDiacriticsAndLowercases(string input, string expected)
    {
        InternetSeedRuleExtensions.Sanitize(input).Should().Be(expected);
    }

    [TestCase("Mary Jane", "mary.jane")]
    [TestCase("Ana Paula", "ana.paula")]
    public void Sanitize_ReplacesSpacesWithDots(string input, string expected)
    {
        InternetSeedRuleExtensions.Sanitize(input).Should().Be(expected);
    }

    [TestCase("jean-paul", "jean.paul")]
    [TestCase("first_last", "first.last")]
    public void Sanitize_ReplacesHyphensAndUnderscoresWithDots(string input, string expected)
    {
        InternetSeedRuleExtensions.Sanitize(input).Should().Be(expected);
    }

    [TestCase(" alice ", "alice")]
    [TestCase(".alice.", "alice")]
    public void Sanitize_TrimsLeadingAndTrailingDots(string input, string expected)
    {
        InternetSeedRuleExtensions.Sanitize(input).Should().Be(expected);
    }

    [TestCase("alice!#$%", "alice")]
    public void Sanitize_RemovesNonAlphanumericCharacters(string input, string expected)
    {
        InternetSeedRuleExtensions.Sanitize(input).Should().Be(expected);
    }

    #endregion

    #region UseEmail (random)

    [Test]
    public void UseEmail_ReturnsParentBuilder()
    {
        // Arrange
        var builder = new SeedBuilder<User>();
        var rule = builder.RuleFor(u => u.Email);

        // Act
        var result = rule.UseEmail();

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void UseEmail_ProducesNonEmptyEmail()
    {
        // Arrange
        var rule = CreateEmailRule();
        rule.UseEmail();

        // Act & Assert
        ApplyOnce(rule).Should().NotBeNullOrEmpty();
    }

    [Test]
    public void UseEmail_ProducesEmailWithExactlyOneAtSign()
    {
        // Arrange
        var rule = CreateEmailRule();
        rule.UseEmail();

        // Act
        var email = ApplyOnce(rule);

        // Assert
        email.Count(c => c == '@').Should().Be(1);
    }

    [Test]
    public void UseEmail_ProducesLowercaseEmail()
    {
        // Arrange
        var rule = CreateEmailRule();
        rule.UseEmail();

        // Act
        var email = ApplyOnce(rule);

        // Assert
        email.Should().Be(email.ToLowerInvariant());
    }

    [Test]
    public void UseEmail_ProducesNonEmptyLocalAndDomainParts()
    {
        // Arrange
        var rule = CreateEmailRule();
        rule.UseEmail();

        // Act
        var parts = ApplyOnce(rule).Split('@');

        // Assert
        parts[0].Should().NotBeEmpty();
        parts[1].Should().NotBeEmpty();
    }

    #endregion

    #region UseEmail (fixed suffix)

    [Test]
    public void UseEmail_WithSuffix_ReturnsParentBuilder()
    {
        // Arrange
        var builder = new SeedBuilder<User>();
        var rule = builder.RuleFor(u => u.Email);

        // Act
        var result = rule.UseEmail("example.com");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void UseEmail_WithSuffix_ProducesEmailEndingWithSuffix()
    {
        // Arrange
        var rule = CreateEmailRule();
        rule.UseEmail("mycompany.com");

        // Act
        var email = ApplyOnce(rule);

        // Assert
        email.Should().EndWith("@mycompany.com");
    }

    [Test]
    public void UseEmail_WithSuffix_ProducesNonEmptyPrefix()
    {
        // Arrange
        var rule = CreateEmailRule();
        rule.UseEmail("example.com");

        // Act
        var local = ApplyOnce(rule).Split('@')[0];

        // Assert
        local.Should().NotBeEmpty();
    }

    [Test]
    public void UseEmail_WithSuffix_ProducesLowercaseEmail()
    {
        // Arrange
        var rule = CreateEmailRule();
        rule.UseEmail("example.com");

        // Act
        var email = ApplyOnce(rule);

        // Assert
        email.Should().Be(email.ToLowerInvariant());
    }

    [Test]
    public void UseEmail_WithSuffix_DifferentEntitiesCanReceiveDifferentPrefixes()
    {
        // Arrange
        var rule = CreateEmailRule();
        rule.UseEmail("example.com");

        var emails = Enumerable.Range(0, 20)
            .Select(_ => ApplyOnce(rule))
            .ToList();

        // Assert
        emails.Distinct().Should().HaveCountGreaterThan(1);
    }

    #endregion

    #region UseEmail (entity-based prefix)

    [Test]
    public void UseEmail_WithEntityPrefix_ReturnsParentBuilder()
    {
        // Arrange
        var builder = new SeedBuilder<User>();
        var rule = builder.RuleFor(u => u.Email);

        // Act
        var result = rule.UseEmail(u => u.Name, "example.com");

        // Assert
        result.Should().BeSameAs(builder);
    }

    [Test]
    public void UseEmail_WithEntityPrefix_BuildsEmailFromProperty()
    {
        // Arrange
        var user = new User { Name = "Alice" };
        var rule = CreateEmailRule();
        rule.UseEmail(u => u.Name, "example.com");

        // Act
        rule.Apply(user);

        // Assert
        user.Email.Should().Be("alice@example.com");
    }

    [Test]
    public void UseEmail_WithEntityPrefix_SanitizesDiacritics()
    {
        // Arrange
        var user = new User { Name = "João" };
        var rule = CreateEmailRule();
        rule.UseEmail(u => u.Name, "example.com");

        // Act
        rule.Apply(user);

        // Assert
        user.Email.Should().Be("joao@example.com");
    }

    [Test]
    public void UseEmail_WithEntityPrefix_SanitizesSpacesInConcatenatedExpression()
    {
        // Arrange — expression combines two properties with a dot separator
        var user = new User { Name = "María García" };
        var rule = CreateEmailRule();
        rule.UseEmail(u => u.Name, "example.com");

        // Act
        rule.Apply(user);

        // Assert
        user.Email.Should().Be("maria.garcia@example.com");
    }

    [Test]
    public void UseEmail_WithEntityPrefix_RegistersAccessedPropertiesAsDependencies()
    {
        // Arrange
        var rule = CreateEmailRule();

        // Act
        rule.UseEmail(u => u.Name, "example.com");

        // Assert
        rule.Dependencies.Should().Contain(nameof(User.Name));
    }

    #endregion
}
