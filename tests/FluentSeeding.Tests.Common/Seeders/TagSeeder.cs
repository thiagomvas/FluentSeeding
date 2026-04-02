namespace FluentSeeding.Tests.Common.Seeders;

public sealed class TagSeeder : EntitySeeder<Tag>
{
    protected override void Configure(SeedBuilder<Tag> builder)
    {
        builder.Count(4);
        builder.RuleFor(t => t.Id).UseFactory(Guid.NewGuid);
        builder.RuleFor(t => t.Name).UseFactory(() => $"Tag-{Guid.NewGuid()}");
    }
}
