using FluentSeeding;
using FluentSeeding.Extensions;
using FluentSeeding.Faker.Extensions;
using FluentSeeding.Samples.AspNetCore.Entities;

namespace FluentSeeding.Samples.AspNetCore.Seeders;

public class OrderSeeder : EntitySeeder<Order>
{
    private readonly CustomerSeeder _customerSeeder;
    private readonly ProductSeeder _productSeeder;
    private readonly CouponSeeder _couponSeeder;

    public OrderSeeder(CustomerSeeder customerSeeder, ProductSeeder productSeeder, CouponSeeder couponSeeder)
    {
        _customerSeeder = customerSeeder;
        _productSeeder = productSeeder;
        _couponSeeder = couponSeeder;
    }

    protected override void Configure(SeedBuilder<Order> builder)
    {
        var customers = _customerSeeder.Data;
        var products  = _productSeeder.Data;
        var activeCouponIds = _couponSeeder.Data.Where(c => c.IsActive).Select(c => c.Id).ToArray();

        builder.Count(40);
        builder.RuleFor(x => x.Id).UseIdempotentGuid();
        builder.RuleFor(x => x.CustomerId).UseFrom(customers.Select(c => c.Id));
        builder.RuleFor(x => x.CouponId).UseFactory(() =>
            Random.Shared.NextDouble() > 0.6 ? activeCouponIds[Random.Shared.Next(activeCouponIds.Length)] : (Guid?)null);
        builder.RuleFor(x => x.ShippingAddress).UseFullAddress();
        builder.RuleFor(x => x.PlacedAt).UsePast(years: 2);
        builder.RuleFor(x => x.Status).UseFrom(
            OrderStatus.Pending, OrderStatus.Processing,
            OrderStatus.Shipped, OrderStatus.Delivered, OrderStatus.Cancelled);
        builder.RuleFor(x => x.ShippedAt).UseFactory(() =>
            Random.Shared.NextDouble() > 0.3 ? DateTime.UtcNow.AddDays(-Random.Shared.Next(1, 60)) : (DateTime?)null);
        builder.HasMany(x => x.OrderItems, items =>
        {
            items.Count(1, 6);
            items.RuleFor(i => i.Id).UseIdempotentGuid("orderitem");
            items.RuleFor(i => i.ProductId).UseFrom(products.Select(p => p.Id));
            items.RuleFor(i => i.Quantity).UseRandom(min: 1, max: 10);
            items.RuleFor(i => i.UnitPrice).UseFactory(
                () => Math.Round((decimal)(Random.Shared.NextDouble() * 495 + 5), 2));
        });
    }
}
