using FluentSeeding;
using FluentSeeding.Samples.AspNetCore.Entities;

namespace FluentSeeding.Samples.AspNetCore.Seeders;

public class OrderSeeder : EntitySeeder<Order>
{
    private readonly CustomerSeeder _customerSeeder;
    private readonly ProductSeeder _productSeeder;

    public OrderSeeder(CustomerSeeder customerSeeder, ProductSeeder productSeeder)
    {
        _customerSeeder = customerSeeder;
        _productSeeder = productSeeder;
    }

    protected override void Configure(SeedBuilder<Order> builder)
    {
        var products = _productSeeder.Data;

        builder
            .Count(15)
            .RuleFor(x => x.Id).UseFactory(Guid.NewGuid)
            .RuleFor(x => x.CustomerId).UseFrom(_customerSeeder.Data.Select(c => c.Id))
            .RuleFor(x => x.CreatedAt).UseFactory(() => DateTime.UtcNow.AddDays(-Random.Shared.Next(0, 365)))
            .RuleFor(x => x.Status).UseFrom(OrderStatus.Pending, OrderStatus.Processing, OrderStatus.Completed, OrderStatus.Cancelled)
            .HasMany(x => x.OrderItems, items => items
                .Count(1, 5)
                .RuleFor(i => i.Id).UseFactory(Guid.NewGuid)
                .RuleFor(i => i.ProductId).UseFrom(products.Select(p => p.Id))
                .RuleFor(i => i.Quantity).UseFactory(() => Random.Shared.Next(1, 10))
                .RuleFor(i => i.UnitPrice).UseFactory(() => Math.Round((decimal)(Random.Shared.NextDouble() * 195 + 5), 2))
            );
    }
}
