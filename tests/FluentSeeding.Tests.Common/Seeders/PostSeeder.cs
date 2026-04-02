namespace FluentSeeding.Tests.Common.Seeders;

public sealed class PostSeeder(AuthorSeeder authorSeeder) : EntitySeeder<Post>
{
    protected override void Configure(SeedBuilder<Post> builder)
    {
        builder.Count(5);
        builder.RuleFor(p => p.Id).UseFactory(Guid.NewGuid)
            .RuleFor(p => p.Title).UseValue("Test Post")
            .RuleFor(p => p.Content).UseValue("Test Content")
            .RuleFor(p => p.AuthorId).UseFrom(authorSeeder.Data.Select(a => a.Id));
    }
}
