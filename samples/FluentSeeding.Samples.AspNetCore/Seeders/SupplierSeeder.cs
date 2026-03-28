using FluentSeeding;
using FluentSeeding.Faker.Extensions;
using FluentSeeding.Samples.AspNetCore.Entities;

namespace FluentSeeding.Samples.AspNetCore.Seeders;

public class SupplierSeeder : EntitySeeder<Supplier>
{
    private static readonly string[] Names =
    [
        "Peak Supplies Inc.", "BlueLine Wholesale", "NovaTrade Co.",
        "Sterling Distributors", "Apex Group Ltd.", "Brightfield LLC",
    ];

    protected override void Configure(SeedBuilder<Supplier> builder)
    {
        builder.Count(6);
        builder.RuleFor(x => x.Id).UseIdempotentGuid();
        builder.RuleFor(x => x.Name).UseFactory(i => Names[i]);
        builder.RuleFor(x => x.ContactEmail).UseEmail();
        builder.RuleFor(x => x.Phone).UseFactory(() =>
        {
            var area = Random.Shared.Next(200, 999);
            var mid  = Random.Shared.Next(100, 999);
            var last = Random.Shared.Next(1000, 9999);
            return $"({area}) {mid}-{last}";
        });
        builder.RuleFor(x => x.Address).UseStreetAddress();
    }
}
