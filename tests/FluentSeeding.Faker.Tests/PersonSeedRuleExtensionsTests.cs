using FluentAssertions;
using FluentSeeding.Faker.Extensions;
using FluentSeeding.Tests.Common;

namespace FluentSeeding.Faker.Tests;

[TestFixture(TestName = "PersonSeedRuleExtensions")]
[Category("Unit")]
[Category(nameof(PersonSeedRuleExtensions))]
public sealed class PersonSeedRuleExtensionsTests
{
    private static SeedRule<User, string> CreateStringRule(string? locale = null)
    {
        var builder = new SeedBuilder<User>();
        if (locale is not null)
            builder.WithLocale(locale);
        return builder.RuleFor(u => u.Name);
    }

    private static string ApplyOnce(SeedRule<User, string> rule)
    {
        var user = new User();
        rule.Apply(user);
        return user.Name;
    }

    [Test]
    public void UseFirstName_WithDefaultSettings_SetsNonEmptyName()
    {
        var rule = CreateStringRule();
        rule.UseFirstName();
        ApplyOnce(rule).Should().NotBeNullOrEmpty();
    }

    [Test]
    public void UseFirstName_WithGenderMale_SetsNonEmptyName()
    {
        var rule = CreateStringRule();
        rule.UseFirstName(Gender.Male);
        ApplyOnce(rule).Should().NotBeNullOrEmpty();
    }

    [Test]
    public void UseFirstName_WithGenderFemale_SetsNonEmptyName()
    {
        var rule = CreateStringRule();
        rule.UseFirstName(Gender.Female);
        ApplyOnce(rule).Should().NotBeNullOrEmpty();
    }

    [Test]
    public void UseFirstName_WithGenderAny_SetsNonEmptyName()
    {
        var rule = CreateStringRule();
        rule.UseFirstName(Gender.Any);
        ApplyOnce(rule).Should().NotBeNullOrEmpty();
    }

    [Test]
    public void UseFirstName_WithLocaleOnBuilder_SetsNonEmptyName()
    {
        var rule = CreateStringRule(locale: "en");
        rule.UseFirstName();
        ApplyOnce(rule).Should().NotBeNullOrEmpty();
    }

    [Test]
    public void UseLastName_WithDefaultSettings_SetsNonEmptyName()
    {
        var rule = CreateStringRule();
        rule.UseLastName();
        ApplyOnce(rule).Should().NotBeNullOrEmpty();
    }

    [Test]
    public void UseLastName_WithLocaleOnBuilder_SetsNonEmptyName()
    {
        var rule = CreateStringRule(locale: "en");
        rule.UseLastName();
        ApplyOnce(rule).Should().NotBeNullOrEmpty();
    }

    [Test]
    public void UseFullName_WithDefaultSettings_SetsNameContainingSpace()
    {
        var rule = CreateStringRule();
        rule.UseFullName();
        ApplyOnce(rule).Should().Contain(" ");
    }

    [Test]
    public void UseFullName_WithGenderMale_SetsNameContainingSpace()
    {
        var rule = CreateStringRule();
        rule.UseFullName(Gender.Male);
        ApplyOnce(rule).Should().Contain(" ");
    }

    [Test]
    public void UseFullName_WithGenderFemale_SetsNameContainingSpace()
    {
        var rule = CreateStringRule();
        rule.UseFullName(Gender.Female);
        ApplyOnce(rule).Should().Contain(" ");
    }

    [Test]
    public void UseFullName_WithLocaleOnBuilder_SetsNameContainingSpace()
    {
        var rule = CreateStringRule(locale: "en");
        rule.UseFullName();
        ApplyOnce(rule).Should().Contain(" ");
    }
}
