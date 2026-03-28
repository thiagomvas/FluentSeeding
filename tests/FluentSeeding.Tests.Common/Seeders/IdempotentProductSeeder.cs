namespace FluentSeeding.Tests.Common.Seeders;

public sealed class IdempotentProductSeeder : EntitySeeder<Product>
{
    protected override void Configure(SeedBuilder<Product> builder)
    {
        builder.Count(3);
        builder.RuleFor(p => p.Id).UseIdempotentGuid();
        builder.RuleFor(p => p.Name).UseValue("Idempotent Product");
        builder.RuleFor(p => p.Price).UseValue(1.00m);
    }
}
