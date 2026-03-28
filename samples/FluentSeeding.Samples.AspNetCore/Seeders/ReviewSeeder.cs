using FluentSeeding;
using FluentSeeding.Extensions;
using FluentSeeding.Faker.Extensions;
using FluentSeeding.Samples.AspNetCore.Entities;

namespace FluentSeeding.Samples.AspNetCore.Seeders;

public class ReviewSeeder : EntitySeeder<ProductReview>
{
    private static readonly string[] Comments =
    [
        "Absolutely love this product — great quality and solid design.",
        "Good value for money. The finish is a nice touch.",
        "Works exactly as described. Would definitely recommend.",
        "Not bad, but expected something a bit more premium for the price.",
        "Outstanding build quality — worth every penny.",
        "Decent product, does the job without any fuss.",
        "Very satisfied with my purchase. Will buy again.",
        "Surprised by how well it performs. Exceeded my expectations.",
        "Arrived quickly and in perfect condition. Happy customer!",
        "A solid choice if you're on a budget.",
    ];

    private readonly ProductSeeder _productSeeder;
    private readonly CustomerSeeder _customerSeeder;

    public ReviewSeeder(ProductSeeder productSeeder, CustomerSeeder customerSeeder)
    {
        _productSeeder = productSeeder;
        _customerSeeder = customerSeeder;
    }

    protected override void Configure(SeedBuilder<ProductReview> builder)
    {
        var productIds  = _productSeeder.Data.Select(p => p.Id).ToArray();
        var customerIds = _customerSeeder.Data.Select(c => c.Id).ToArray();

        builder.Count(60);
        builder.RuleFor(x => x.Id).UseIdempotentGuid();
        builder.RuleFor(x => x.ProductId).UseFrom(productIds);
        builder.RuleFor(x => x.CustomerId).UseFrom(customerIds);
        builder.RuleFor(x => x.Rating).UseRandom(min: 1, max: 6);
        builder.RuleFor(x => x.Comment).UseFrom(Comments);
        builder.RuleFor(x => x.CreatedAt).UsePast(years: 2);
    }
}
