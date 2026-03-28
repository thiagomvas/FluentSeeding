using FluentSeeding;
using FluentSeeding.Extensions;
using FluentSeeding.Faker.Extensions;
using FluentSeeding.Samples.AspNetCore.Entities;

namespace FluentSeeding.Samples.AspNetCore.Seeders;

public class ProductSeeder : EntitySeeder<Product>
{
    private static readonly string[] Names =
    [
        "Laptop", "T-Shirt", "C# in Depth", "Coffee Maker", "Yoga Mat",
        "Smartphone", "Jeans", "Design Patterns", "Blender", "Running Shoes",
        "Headphones", "Jacket", "Clean Code", "Air Fryer", "Tennis Racket",
        "Tablet", "Sneakers", "The Pragmatic Programmer", "Rice Cooker", "Dumbbells",
        "Wireless Mouse", "Dress Shirt", "JavaScript: The Good Parts", "Toaster", "Jump Rope",
        "Monitor", "Cargo Pants", "Refactoring", "Espresso Machine", "Bicycle Helmet",
    ];

    private static readonly string[] Descriptions =
    [
        "Compact and reliable for daily use.",
        "Built to last with premium materials.",
        "Ergonomic design for maximum comfort.",
        "Lightweight and portable — take it anywhere.",
        "Energy-efficient with a modern look.",
        "Perfect for beginners and professionals alike.",
        "Comes with a 2-year manufacturer warranty.",
        "High performance at an unbeatable price.",
    ];

    private readonly CategorySeeder _categorySeeder;
    private readonly SupplierSeeder _supplierSeeder;

    public ProductSeeder(CategorySeeder categorySeeder, SupplierSeeder supplierSeeder)
    {
        _categorySeeder = categorySeeder;
        _supplierSeeder = supplierSeeder;
    }

    protected override void Configure(SeedBuilder<Product> builder)
    {
        builder.Count(30);
        builder.RuleFor(x => x.Id).UseIdempotentGuid();
        builder.RuleFor(x => x.Name).UseFactory(i => Names[i]);
        builder.RuleFor(x => x.Description).UseFrom(Descriptions);
        builder.RuleFor(x => x.Price).UseFactory(() => Math.Round((decimal)(Random.Shared.NextDouble() * 495 + 5), 2));
        builder.RuleFor(x => x.StockQuantity).UseRandom(min: 0, max: 500);
        builder.RuleFor(x => x.CategoryId).UseFrom(_categorySeeder.Data.Select(c => c.Id));
        builder.RuleFor(x => x.SupplierId).UseFrom(_supplierSeeder.Data.Select(s => s.Id));
    }
}
