using FluentSeeding;
using FluentSeeding.Faker.Extensions;
using FluentSeeding.Samples.AspNetCore.Entities;

namespace FluentSeeding.Samples.AspNetCore.Seeders;

public class CustomerSeeder : EntitySeeder<Customer>
{
    protected override void Configure(SeedBuilder<Customer> builder)
    {
        builder.Count(20);
        builder.RuleFor(x => x.Id).UseIdempotentGuid();
        builder.RuleFor(x => x.FirstName).UseFirstName();
        builder.RuleFor(x => x.LastName).UseLastName();
        builder.RuleFor(x => x.Email).UseEmail(x => x.FirstName + "." + x.LastName, "example.com");
        builder.RuleFor(x => x.Phone).UseFactory(() =>
        {
            var area = Random.Shared.Next(200, 999);
            var mid  = Random.Shared.Next(100, 999);
            var last = Random.Shared.Next(1000, 9999);
            return $"({area}) {mid}-{last}";
        });
        builder.RuleFor(x => x.ShippingAddress).UseFullAddress();
        builder.RuleFor(x => x.MemberSince).UsePast(years: 5);
        builder.RuleFor(x => x.IsActive).UseFactory(() => Random.Shared.NextDouble() > 0.15);
    }
}
