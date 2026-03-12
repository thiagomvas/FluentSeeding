namespace FluentSeeding.Tests.Common.Seeders;

public sealed class ProductSeeder : EntitySeeder<Product>
{
    protected override void Configure(SeedBuilder<Product> builder)
    {
        builder.Count(5);
        builder.RuleFor(p => p.Id).UseFactory(Guid.NewGuid);
        builder.RuleFor(p => p.Name).UseValue("Sample Product");
        builder.RuleFor(p => p.Price).UseValue(9.99m);
    }
}
