namespace FluentSeeding.Tests.Common.Seeders;

public sealed class PurchaseSeeder(UserSeeder userSeeder, ProductSeeder productSeeder) : EntitySeeder<Purchase>
{
    protected override void Configure(SeedBuilder<Purchase> builder)
    {
        builder.RuleFor(p => p.Id).UseFactory(Guid.NewGuid)
            .RuleFor(p => p.UserId).UseFrom(userSeeder.Data.Select(u => u.Id))
            .RuleFor(p => p.ProductId).UseFrom(productSeeder.Data.Select(p => p.Id))
            .RuleFor(p => p.Quantity).UseFactory(() => Random.Shared.Next(1, 100));
    }
}
