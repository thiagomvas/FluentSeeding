namespace FluentSeeding.Faker.Extensions;

public static class WordSeedRuleExtensions
{
    /// <summary>
    /// Picks a random adjective drawn from the builder's locale.
    /// </summary>
    public static SeedBuilder<T> UseAdjective<T>(this SeedRule<T, string> rule) where T : class
    {
        return rule.UseFrom(FluentFaker.Locale(rule.Parent.GetLocale()).Words.Adjectives);
    }

    /// <summary>
    /// Picks a random noun drawn from the builder's locale.
    /// </summary>
    public static SeedBuilder<T> UseNoun<T>(this SeedRule<T, string> rule) where T : class
    {
        return rule.UseFrom(FluentFaker.Locale(rule.Parent.GetLocale()).Words.Nouns);
    }

    /// <summary>
    /// Picks a random verb (base/infinitive form) drawn from the builder's locale.
    /// </summary>
    public static SeedBuilder<T> UseVerb<T>(this SeedRule<T, string> rule) where T : class
    {
        return rule.UseFrom(FluentFaker.Locale(rule.Parent.GetLocale()).Words.Verbs);
    }

    /// <summary>
    /// Picks a random adverb drawn from the builder's locale.
    /// </summary>
    public static SeedBuilder<T> UseAdverb<T>(this SeedRule<T, string> rule) where T : class
    {
        return rule.UseFrom(FluentFaker.Locale(rule.Parent.GetLocale()).Words.Adverbs);
    }

    /// <summary>
    /// Picks a random color name drawn from the builder's locale.
    /// </summary>
    public static SeedBuilder<T> UseColor<T>(this SeedRule<T, string> rule) where T : class
    {
        return rule.UseFrom(FluentFaker.Locale(rule.Parent.GetLocale()).Words.Colors);
    }

    /// <summary>
    /// Picks a random animal name drawn from the builder's locale.
    /// </summary>
    public static SeedBuilder<T> UseAnimal<T>(this SeedRule<T, string> rule) where T : class
    {
        return rule.UseFrom(FluentFaker.Locale(rule.Parent.GetLocale()).Words.Animals);
    }
}
