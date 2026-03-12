using FluentAssertions;
using FluentSeeding.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FluentSeeding.DependencyInjection.Tests.ServiceCollectionExtensions;

[TestFixture(TestName = "ServiceCollectionExtensions.AddFluentSeeding")]
[Category("Unit")]
[Category(nameof(global::FluentSeeding.DependencyInjection.ServiceCollectionExtensions))]
public sealed class ServiceCollectionExtensionsAddFluentSeedingTests
{
    private class User
    {
        public string Name { get; set; } = string.Empty;
    }

    private class UserSeeder : EntitySeeder<User>
    {
        protected override void Configure(SeedBuilder<User> builder) => builder.Count(1);
    }

    private static IServiceCollection BuildServices(Action<global::FluentSeeding.DependencyInjection.SeederBuilder>? configure = null)
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddScoped(_ => Substitute.For<IPersistenceLayer>());
        services.AddFluentSeeding(configure ?? (_ => { }));
        return services;
    }

    [Test]
    public void AddFluentSeeding_RegistersSeederRunner()
    {
        // Arrange
        var services = BuildServices();

        // Act
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var runner = scope.ServiceProvider.GetService<SeederRunner>();

        // Assert
        runner.Should().NotBeNull();
    }

    [Test]
    public void AddFluentSeeding_InvokesConfigureAction()
    {
        // Arrange
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddScoped(_ => Substitute.For<IPersistenceLayer>());
        var configureInvoked = false;

        // Act
        services.AddFluentSeeding(_ => configureInvoked = true);

        // Assert
        configureInvoked.Should().BeTrue();
    }

    [Test]
    public void AddFluentSeeding_ReturnsServicesForChaining()
    {
        // Arrange
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        // Act
        var returned = services.AddFluentSeeding(_ => { });

        // Assert
        returned.Should().BeSameAs(services);
    }

    [Test]
    public void AddFluentSeeding_SeederRunner_IsScoped()
    {
        // Arrange
        var services = BuildServices(b => b.AddSeeder<UserSeeder>());

        // Act
        using var provider = services.BuildServiceProvider();
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();
        var runner1a = scope1.ServiceProvider.GetRequiredService<SeederRunner>();
        var runner1b = scope1.ServiceProvider.GetRequiredService<SeederRunner>();
        var runner2 = scope2.ServiceProvider.GetRequiredService<SeederRunner>();

        // Assert
        runner1a.Should().BeSameAs(runner1b);
        runner1a.Should().NotBeSameAs(runner2);
    }
}
