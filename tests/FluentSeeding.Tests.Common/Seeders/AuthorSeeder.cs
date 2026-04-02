namespace FluentSeeding.Tests.Common.Seeders;

public sealed class AuthorSeeder : EntitySeeder<Author>
{
    protected override void Configure(SeedBuilder<Author> builder)
    {
        builder.Count(3);
        builder.RuleFor(a => a.Id).UseFactory(Guid.NewGuid);
        builder.RuleFor(a => a.Name).UseValue("Test Author");
    }
}
