using FluentSeeding;
using FluentSeeding.Faker.Extensions;
using FluentSeeding.Samples.AspNetCore.Entities;

namespace FluentSeeding.Samples.AspNetCore.Seeders;

public class CouponSeeder : EntitySeeder<Coupon>
{
    private static readonly (string Code, decimal Discount)[] Coupons =
    [
        ("WELCOME10", 10m),
        ("SUMMER20",  20m),
        ("FLASH15",   15m),
        ("VIP25",     25m),
        ("SAVE5",      5m),
    ];

    protected override void Configure(SeedBuilder<Coupon> builder)
    {
        builder.Count(5);
        builder.RuleFor(x => x.Id).UseIdempotentGuid();
        builder.RuleFor(x => x.Code).UseFactory(i => Coupons[i].Code);
        builder.RuleFor(x => x.DiscountPercent).UseFactory(i => Coupons[i].Discount);
        builder.RuleFor(x => x.ExpiresAt).UseFuture(years: 2);
        builder.RuleFor(x => x.IsActive).UseFactory(i => i < 3);
    }
}
