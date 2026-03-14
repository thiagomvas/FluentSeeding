using FluentSeeding;
using FluentSeeding.Samples.AspNetCore.Entities;

namespace FluentSeeding.Samples.AspNetCore.Seeders;

public class ProductSeeder : EntitySeeder<Product>
{
    private static readonly string[] Names =
    [
        "Laptop", "T-Shirt", "C# in Depth", "Coffee Maker", "Yoga Mat",
        "Smartphone", "Jeans", "Design Patterns", "Blender", "Running Shoes",
        "Headphones", "Jacket", "Clean Code", "Air Fryer", "Tennis Racket",
        "Tablet", "Sneakers", "The Pragmatic Programmer", "Rice Cooker", "Dumbbells"
    ];

    private readonly CategorySeeder _categorySeeder;

    public ProductSeeder(CategorySeeder categorySeeder)
    {
        _categorySeeder = categorySeeder;
    }

    protected override void Configure(SeedBuilder<Product> builder)
    {
        builder
            .Count(20)
            .RuleFor(x => x.Id).UseFactory(Guid.NewGuid)
            .RuleFor(x => x.Name).UseFactory(i => Names[i])
            .RuleFor(x => x.Price).UseFactory(() => Math.Round((decimal)(Random.Shared.NextDouble() * 195 + 5), 2))
            .RuleFor(x => x.CategoryId).UseFrom(_categorySeeder.Data.Select(c => c.Id));
    }
}
