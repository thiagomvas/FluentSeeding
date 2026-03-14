using FluentSeeding;
using FluentSeeding.Samples.AspNetCore.Entities;

namespace FluentSeeding.Samples.AspNetCore.Seeders;

public class CategorySeeder : EntitySeeder<Category>
{
    private static readonly string[] Names =
        ["Electronics", "Clothing", "Books", "Home & Kitchen", "Sports"];

    protected override void Configure(SeedBuilder<Category> builder)
    {
        builder
            .Count(5)
            .RuleFor(x => x.Id).UseFactory(Guid.NewGuid)
            .RuleFor(x => x.Name).UseFactory(i => Names[i]);
    }
}
