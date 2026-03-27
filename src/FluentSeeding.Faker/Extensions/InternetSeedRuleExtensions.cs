using System.Globalization;
using System.Linq.Expressions;
using System.Text;
using FluentSeeding.Faker.Locales;

namespace FluentSeeding.Faker.Extensions;

public static class InternetSeedRuleExtensions
{
    /// <summary>
    /// Generates a random email address using a name and email suffix drawn from the builder's locale
    /// (set via <c>WithLocale</c>, falling back to <see cref="FluentFaker.DefaultLocale"/>).
    /// </summary>
    public static SeedBuilder<T> UseEmail<T>(this SeedRule<T, string> rule)
        where T : class
    {
        return rule.UseFactory(() =>
        {
            var data = FluentFaker.Locale(rule.Parent.GetLocale());
            var prefix = BuildRandomPrefix(data);
            var suffix = data.Internet.EmailSuffix.Pick();
            return $"{prefix}@{suffix}";
        });
    }

    /// <summary>
    /// Generates a random email address with a random name prefix drawn from the builder's locale and a
    /// fixed <paramref name="suffix"/> (e.g. <c>jane.doe@mycompany.com</c>).
    /// </summary>
    public static SeedBuilder<T> UseEmail<T>(this SeedRule<T, string> rule, string suffix)
        where T : class
    {
        return rule.UseFactory(() =>
        {
            var data = FluentFaker.Locale(rule.Parent.GetLocale());
            var prefix = BuildRandomPrefix(data);
            return $"{prefix}@{suffix}";
        });
    }

    /// <summary>
    /// Generates an email address by sanitizing the result of <paramref name="prefix"/> evaluated on the entity
    /// and appending <paramref name="suffix"/> (e.g. <c>u => u.FirstName + "." + u.LastName</c> →
    /// <c>john.doe@example.com</c>). Direct property accesses in the expression are automatically registered
    /// as dependencies.
    /// </summary>
    public static SeedBuilder<T> UseEmail<T>(this SeedRule<T, string> rule,
        Expression<Func<T, string>> prefix, string suffix)
        where T : class
    {
        var sanitizeMethod = typeof(InternetSeedRuleExtensions)
            .GetMethod(nameof(Sanitize), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!;

        var atSuffix = Expression.Constant("@" + suffix);
        var sanitizeCall = Expression.Call(sanitizeMethod, prefix.Body);
        var concat = Expression.Call(
            typeof(string).GetMethod(nameof(string.Concat), [typeof(string), typeof(string)])!,
            sanitizeCall, atSuffix);

        var wrapped = Expression.Lambda<Func<T, string>>(concat, prefix.Parameters[0]);
        return rule.UseEntityFactory(wrapped);
    }

    private static string BuildRandomPrefix(LocaleData data)
    {
        var first = Sanitize(data.Person.FirstName.GetForGender(Gender.Any).Pick());
        var last = Sanitize(data.Person.LastName.Pick());
        return $"{first}.{last}";
    }

    internal static string Sanitize(string value)
    {
        var normalized = value.Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);
        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark)
                continue;
            if (char.IsLetterOrDigit(c))
                sb.Append(char.ToLowerInvariant(c));
            else if (c is ' ' or '.' or '-' or '_')
                sb.Append('.');
        }
        return sb.ToString().Trim('.');
    }
}
