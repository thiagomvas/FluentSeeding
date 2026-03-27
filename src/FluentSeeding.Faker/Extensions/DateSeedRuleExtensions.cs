namespace FluentSeeding.Faker.Extensions;

public static class DateSeedRuleExtensions
{
    /// <summary>
    /// Generates a random <see cref="DateOnly"/> in the past, within <paramref name="years"/> years from today.
    /// </summary>
    public static SeedBuilder<T> UsePast<T>(this SeedRule<T, DateOnly> rule, int years = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var from = today.AddYears(-years);
            var days = (today.ToDateTime(TimeOnly.MinValue) - from.ToDateTime(TimeOnly.MinValue)).Days;
            return from.AddDays(Random.Shared.Next(days));
        });
    }

    /// <summary>
    /// Generates a random <see cref="DateOnly"/> in the future, within <paramref name="years"/> years from today.
    /// </summary>
    public static SeedBuilder<T> UseFuture<T>(this SeedRule<T, DateOnly> rule, int years = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var to = today.AddYears(years);
            var days = (to.ToDateTime(TimeOnly.MinValue) - today.ToDateTime(TimeOnly.MinValue)).Days;
            return today.AddDays(Random.Shared.Next(1, days + 1));
        });
    }

    /// <summary>
    /// Generates a random <see cref="DateOnly"/> within the last <paramref name="days"/> days, inclusive of today.
    /// </summary>
    public static SeedBuilder<T> UseRecent<T>(this SeedRule<T, DateOnly> rule, int days = 7) where T : class
    {
        return rule.UseFactory(() =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return today.AddDays(-Random.Shared.Next(0, days + 1));
        });
    }

    /// <summary>
    /// Generates a random <see cref="DateOnly"/> within the next <paramref name="days"/> days, inclusive of today.
    /// </summary>
    public static SeedBuilder<T> UseSoon<T>(this SeedRule<T, DateOnly> rule, int days = 7) where T : class
    {
        return rule.UseFactory(() =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            return today.AddDays(Random.Shared.Next(0, days + 1));
        });
    }

    /// <summary>
    /// Generates a random birthdate for a person aged between <paramref name="minAge"/> and
    /// <paramref name="maxAge"/> years (inclusive on both ends).
    /// </summary>
    public static SeedBuilder<T> UseBirthdate<T>(this SeedRule<T, DateOnly> rule, int minAge = 18, int maxAge = 80)
        where T : class
    {
        return rule.UseFactory(() =>
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var from = today.AddYears(-maxAge);
            var to = today.AddYears(-minAge);
            var days = (to.ToDateTime(TimeOnly.MinValue) - from.ToDateTime(TimeOnly.MinValue)).Days;
            return from.AddDays(Random.Shared.Next(days + 1));
        });
    }

    /// <summary>
    /// Generates a random <see cref="DateTime"/> in the past, within <paramref name="years"/> years from now.
    /// </summary>
    public static SeedBuilder<T> UsePast<T>(this SeedRule<T, DateTime> rule, int years = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = DateTime.UtcNow;
            var from = now.AddYears(-years);
            var ticks = (long)(Random.Shared.NextDouble() * (now - from).Ticks);
            return from.AddTicks(ticks);
        });
    }

    /// <summary>
    /// Generates a random <see cref="DateTime"/> in the future, within <paramref name="years"/> years from now.
    /// </summary>
    public static SeedBuilder<T> UseFuture<T>(this SeedRule<T, DateTime> rule, int years = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = DateTime.UtcNow;
            var to = now.AddYears(years);
            var ticks = (long)(Random.Shared.NextDouble() * (to - now).Ticks);
            return now.AddTicks(ticks);
        });
    }

    /// <summary>
    /// Generates a random <see cref="DateTime"/> within the last <paramref name="hours"/> hours from now,
    /// inclusive of now.
    /// </summary>
    public static SeedBuilder<T> UseRecent<T>(this SeedRule<T, DateTime> rule, int hours = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = DateTime.UtcNow;
            var ticks = (long)(Random.Shared.NextDouble() * TimeSpan.FromHours(hours).Ticks);
            return now.AddTicks(-ticks);
        });
    }

    /// <summary>
    /// Generates a random <see cref="DateTime"/> within the next <paramref name="hours"/> hours from now,
    /// inclusive of now.
    /// </summary>
    public static SeedBuilder<T> UseSoon<T>(this SeedRule<T, DateTime> rule, int hours = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = DateTime.UtcNow;
            var ticks = (long)(Random.Shared.NextDouble() * TimeSpan.FromHours(hours).Ticks);
            return now.AddTicks(ticks);
        });
    }

    /// <summary>
    /// Generates a random <see cref="DateTimeOffset"/> in the past, within <paramref name="years"/> years from now.
    /// </summary>
    public static SeedBuilder<T> UsePast<T>(this SeedRule<T, DateTimeOffset> rule, int years = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = DateTimeOffset.UtcNow;
            var from = now.AddYears(-years);
            var ticks = (long)(Random.Shared.NextDouble() * (now - from).Ticks);
            return from.AddTicks(ticks);
        });
    }

    /// <summary>
    /// Generates a random <see cref="DateTimeOffset"/> in the future, within <paramref name="years"/> years from now.
    /// </summary>
    public static SeedBuilder<T> UseFuture<T>(this SeedRule<T, DateTimeOffset> rule, int years = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = DateTimeOffset.UtcNow;
            var to = now.AddYears(years);
            var ticks = (long)(Random.Shared.NextDouble() * (to - now).Ticks);
            return now.AddTicks(ticks);
        });
    }

    /// <summary>
    /// Generates a random <see cref="DateTimeOffset"/> within the last <paramref name="hours"/> hours from now,
    /// inclusive of now.
    /// </summary>
    public static SeedBuilder<T> UseRecent<T>(this SeedRule<T, DateTimeOffset> rule, int hours = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = DateTimeOffset.UtcNow;
            var ticks = (long)(Random.Shared.NextDouble() * TimeSpan.FromHours(hours).Ticks);
            return now.AddTicks(-ticks);
        });
    }

    /// <summary>
    /// Generates a random <see cref="DateTimeOffset"/> within the next <paramref name="hours"/> hours from now,
    /// inclusive of now.
    /// </summary>
    public static SeedBuilder<T> UseSoon<T>(this SeedRule<T, DateTimeOffset> rule, int hours = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = DateTimeOffset.UtcNow;
            var ticks = (long)(Random.Shared.NextDouble() * TimeSpan.FromHours(hours).Ticks);
            return now.AddTicks(ticks);
        });
    }

    /// <summary>
    /// Generates a random <see cref="TimeOnly"/> in the past, within <paramref name="hours"/> hours from now.
    /// Values that cross midnight wrap around to the previous part of the day.
    /// </summary>
    public static SeedBuilder<T> UsePast<T>(this SeedRule<T, TimeOnly> rule, int hours = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            var offset = Random.Shared.Next(1, hours * 60 + 1);
            return now.AddMinutes(-offset);
        });
    }

    /// <summary>
    /// Generates a random <see cref="TimeOnly"/> in the future, within <paramref name="hours"/> hours from now.
    /// Values that cross midnight wrap around to the next part of the day.
    /// </summary>
    public static SeedBuilder<T> UseFuture<T>(this SeedRule<T, TimeOnly> rule, int hours = 1) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            var offset = Random.Shared.Next(1, hours * 60 + 1);
            return now.AddMinutes(offset);
        });
    }

    /// <summary>
    /// Generates a random <see cref="TimeOnly"/> within the last <paramref name="minutes"/> minutes from now,
    /// inclusive of now. Values that cross midnight wrap around.
    /// </summary>
    public static SeedBuilder<T> UseRecent<T>(this SeedRule<T, TimeOnly> rule, int minutes = 60) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            return now.AddMinutes(-Random.Shared.Next(0, minutes + 1));
        });
    }

    /// <summary>
    /// Generates a random <see cref="TimeOnly"/> within the next <paramref name="minutes"/> minutes from now,
    /// inclusive of now. Values that cross midnight wrap around.
    /// </summary>
    public static SeedBuilder<T> UseSoon<T>(this SeedRule<T, TimeOnly> rule, int minutes = 60) where T : class
    {
        return rule.UseFactory(() =>
        {
            var now = TimeOnly.FromDateTime(DateTime.Now);
            return now.AddMinutes(Random.Shared.Next(0, minutes + 1));
        });
    }
}
