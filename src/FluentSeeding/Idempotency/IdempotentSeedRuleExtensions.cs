namespace FluentSeeding;

public static class IdempotentSeedRuleExtensions
{
    public static SeedBuilder<T> UseIdempotentGuid<T>(this SeedRule<T, Guid> rule, string? seed = null)
        where T : class
        => rule.UseFactory(i => Idempotent.Guid<T>(i, seed));

    public static SeedBuilder<T> UseIdempotentInt<T>(this SeedRule<T, int> rule, string? seed = null)
        where T : class
        => rule.UseFactory(i => Idempotent.Int<T>(i, seed));

    public static SeedBuilder<T> UseIdempotentLong<T>(this SeedRule<T, long> rule, string? seed = null)
        where T : class
        => rule.UseFactory(i => Idempotent.Long<T>(i, seed));

    public static SeedBuilder<T> UseIdempotentSlug<T>(this SeedRule<T, string> rule, string? seed = null)
        where T : class
        => rule.UseFactory(i => Idempotent.Slug<T>(i, seed));
}
