namespace FluentSeeding.Tests.Common.Seeders;

public sealed class UserSeeder : EntitySeeder<User>
{
    protected override void Configure(SeedBuilder<User> builder)
    {
        builder.Count(10);
        
        builder.RuleFor(u => u.Id).UseValue(Guid.NewGuid());
        builder.RuleFor(u => u.Name).UseValue("Test User");
        builder.RuleFor(u => u.Email).UseValue("test@email.com");
    }
}
