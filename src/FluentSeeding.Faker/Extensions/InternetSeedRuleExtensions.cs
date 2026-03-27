using System.Globalization;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
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
    
    /// <summary>
    /// Generates a random IPv4 address.
    /// </summary>
    /// <param name="rule">The seed rule to apply the factory to.</param>
    /// <param name="formatted">
    /// When <c>true</c>, returns a dot-notation address (e.g. <c>192.168.1.1</c>).
    /// When <c>false</c>, returns the raw concatenated octets (e.g. <c>192168011</c>).
    /// </param>
    public static SeedBuilder<T> UseIpv4<T>(this SeedRule<T, string> rule, bool formatted = false) where T : class
    {
        return rule.UseFactory(() =>
        {
            Span<byte> bytes = stackalloc byte[4];
            Random.Shared.NextBytes(bytes);

            if (formatted)
                return $"{bytes[0]}.{bytes[1]}.{bytes[2]}.{bytes[3]}";
            return $"{bytes[0]}{bytes[1]}{bytes[2]}{bytes[3]}";
        });
    }
    /// <summary>
    /// Generates a random IPv6 address.
    /// </summary>
    /// <param name="rule">The seed rule to apply the factory to.</param>
    /// <param name="formatted">
    /// When <c>true</c>, returns a colon-separated groups address (e.g. <c>2001:0db8:85a3:0000:0000:8a2e:0370:7334</c>).
    /// When <c>false</c>, returns the raw lowercase hex string (e.g. <c>20010db885a3000000008a2e03707334</c>).
    /// </param>
    public static SeedBuilder<T> UseIpv6<T>(this SeedRule<T, string> rule, bool formatted = false) where T : class
    {
        return rule.UseFactory(() =>
        {
            Span<byte> bytes = stackalloc byte[16];
            Random.Shared.NextBytes(bytes);

            if (!formatted)
                return Convert.ToHexString(bytes).ToLowerInvariant();

            Span<char> chars = stackalloc char[39];
            var pos = 0;
            for (var i = 0; i < 16; i += 2)
            {
                if (i > 0) chars[pos++] = ':';
                var group = (ushort)((bytes[i] << 8) | bytes[i + 1]);
                group.TryFormat(chars[pos..], out var written, "x4");
                pos += written;
            }
            return new string(chars[..pos]);
        });
    }
    
    /// <summary>
    /// Generates a random MAC address.
    /// </summary>
    /// <param name="rule">The seed rule to apply the factory to.</param>
    /// <param name="separator">
    /// The character used to separate each octet. Defaults to <c>:</c> (e.g. <c>00:1A:2B:3C:4D:5E</c>).
    /// Use <c>'-'</c> for Windows-style (e.g. <c>00-1A-2B-3C-4D-5E</c>), or <c>null</c> for no separator
    /// (e.g. <c>001A2B3C4D5E</c>).
    /// </param>
    /// <param name="uppercase">When <c>true</c>, returns uppercase hex digits. Defaults to <c>true</c>.</param>
    public static SeedBuilder<T> UseMacAddress<T>(this SeedRule<T, string> rule, char? separator = ':', bool uppercase = true) where T : class
    {
        return rule.UseFactory(() =>
        {
            Span<byte> bytes = stackalloc byte[6];
            Random.Shared.NextBytes(bytes);

            if (separator is null)
            {
                var hex = Convert.ToHexString(bytes);
                return uppercase ? hex : hex.ToLowerInvariant();
            }

            Span<char> chars = stackalloc char[17]; 
            var pos = 0;
            for (var i = 0; i < 6; i++)
            {
                if (i > 0) chars[pos++] = separator.Value;
                bytes[i].TryFormat(chars[pos..], out var written, uppercase ? "X2" : "x2");
                pos += written;
            }
            return new string(chars[..pos]);
        });
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
