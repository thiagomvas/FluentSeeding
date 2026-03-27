using FluentSeeding.Faker.Locales;

namespace FluentSeeding.Faker.Extensions;

public static class PersonSeedRuleExtensions
{
    /// <summary>
    /// Generates a random first name drawn from the builder's locale, optionally filtered by <paramref name="gender"/>.
    /// </summary>
    public static SeedBuilder<T> UseFirstName<T>(this SeedRule<T, string> rule, Gender gender = Gender.Any) where T : class
    {
        return rule.UseFrom(FluentFaker.Locale(rule.Parent.GetLocale()).Person.FirstName.GetForGender(gender));
    }

    /// <summary>
    /// Generates a random last name drawn from the builder's locale.
    /// </summary>
    public static SeedBuilder<T> UseLastName<T>(this SeedRule<T, string> rule) where T : class
    {
        return rule.UseFrom(FluentFaker.Locale(rule.Parent.GetLocale()).Person.LastName);
    }

    /// <summary>
    /// Generates a random full name drawn from the builder's locale, optionally filtered by <paramref name="gender"/>.
    /// Combines one first name with one or two last names.
    /// </summary>
    public static SeedBuilder<T> UseFullName<T>(this SeedRule<T, string> rule, Gender gender = Gender.Any) where T : class
    {
        return rule.UseFactory(() =>
        {
            var data = FluentFaker.Locale(rule.Parent.GetLocale());
            var firstName = data.Person.FirstName.GetForGender(gender).Pick();
            var lastNames = data.Person.LastName.PickMany(Random.Shared.Next(1, 3)).ToArray();
            return $"{firstName} {string.Join(" ", lastNames)}";
        });
    }
}
