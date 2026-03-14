using FluentSeeding;
using FluentSeeding.Samples.AspNetCore.Entities;

namespace FluentSeeding.Samples.AspNetCore.Seeders;

public class CustomerSeeder : EntitySeeder<Customer>
{
    private static readonly string[] FirstNames =
        ["Alice", "Bob", "Charlie", "Diana", "Eve", "Frank", "Grace", "Henry", "Iris", "Jack"];

    private static readonly string[] LastNames =
        ["Smith", "Johnson", "Williams", "Brown", "Jones", "Garcia", "Miller", "Davis", "Wilson", "Moore"];

    protected override void Configure(SeedBuilder<Customer> builder)
    {
        builder
            .Count(10)
            .RuleFor(x => x.Id).UseFactory(Guid.NewGuid)
            .RuleFor(x => x.FirstName).UseFactory(i => FirstNames[i])
            .RuleFor(x => x.LastName).UseFactory(i => LastNames[i])
            .RuleFor(x => x.Email).UseFactory(i => $"customer{i + 1}@example.com");
    }
}
