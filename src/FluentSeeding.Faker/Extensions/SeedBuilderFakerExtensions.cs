using System.Runtime.CompilerServices;

namespace FluentSeeding.Faker.Extensions;

public static class SeedBuilderFakerExtensions
{
    private static readonly ConditionalWeakTable<object, string> _locales = new();

    /// <summary>
    /// Sets the locale used by all Faker extensions on rules belonging to this builder
    /// (e.g. <c>UseFirstName</c>, <c>UseEmail</c>).
    /// Falls back to <see cref="FluentFaker.DefaultLocale"/> when not set.
    /// </summary>
    public static SeedBuilder<T> WithLocale<T>(this SeedBuilder<T> builder, string locale)
        where T : class
    {
        _locales.AddOrUpdate(builder, locale);
        return builder;
    }

    internal static string GetLocale<T>(this SeedBuilder<T> builder)
        where T : class
        => _locales.TryGetValue(builder, out var locale) ? locale : FluentFaker.DefaultLocale;
}
