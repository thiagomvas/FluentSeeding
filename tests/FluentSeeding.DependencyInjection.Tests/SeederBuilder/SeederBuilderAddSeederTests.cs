using FluentAssertions;
using FluentSeeding.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace FluentSeeding.DependencyInjection.Tests.SeederBuilder;

[TestFixture(TestName = "SeederBuilder.AddSeeder")]
[Category("Unit")]
[Category(nameof(global::FluentSeeding.DependencyInjection.SeederBuilder))]
public sealed class SeederBuilderAddSeederTests
{
    private class User
    {
        public string Name { get; set; } = string.Empty;
    }

    private class Product
    {
        public string Title { get; set; } = string.Empty;
    }

    private class UserSeeder : EntitySeeder<User>
    {
        protected override void Configure(SeedBuilder<User> builder) => builder.Count(1);
    }

    private class ProductSeeder : EntitySeeder<Product>
    {
        protected override void Configure(SeedBuilder<Product> builder) => builder.Count(1);
    }

    private static ServiceProvider BuildProvider(Action<global::FluentSeeding.DependencyInjection.SeederBuilder> configure)
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        services.AddScoped(_ => Substitute.For<IPersistenceLayer>());
        services.AddFluentSeeding(configure);
        return services.BuildServiceProvider();
    }

    [Test]
    public void AddSeeder_RegistersConcreteType()
    {
        // Arrange & Act
        using var provider = BuildProvider(b => b.AddSeeder<UserSeeder>());
        using var scope = provider.CreateScope();

        var seeder = scope.ServiceProvider.GetService<UserSeeder>();

        // Assert
        seeder.Should().NotBeNull();
    }

    [Test]
    public void AddSeeder_RegistersAsEntitySeederBase()
    {
        // Arrange & Act
        using var provider = BuildProvider(b => b.AddSeeder<UserSeeder>());
        using var scope = provider.CreateScope();

        var seeders = scope.ServiceProvider.GetServices<EntitySeederBase>().ToList();

        // Assert
        seeders.Should().ContainSingle(s => s is UserSeeder);
    }

    [Test]
    public void AddSeeder_MultipleRegistrations_AllAppearInEnumerable()
    {
        // Arrange & Act
        using var provider = BuildProvider(b => b.AddSeeder<UserSeeder>().AddSeeder<ProductSeeder>());
        using var scope = provider.CreateScope();

        var seeders = scope.ServiceProvider.GetServices<EntitySeederBase>().ToList();

        // Assert
        seeders.Should().Contain(s => s is UserSeeder);
        seeders.Should().Contain(s => s is ProductSeeder);
    }

    [Test]
    public void AddSeeder_SameInstanceResolvedWithinScope()
    {
        // Arrange & Act
        using var provider = BuildProvider(b => b.AddSeeder<UserSeeder>());
        using var scope = provider.CreateScope();

        var concrete = scope.ServiceProvider.GetRequiredService<UserSeeder>();
        var viaBase = scope.ServiceProvider.GetServices<EntitySeederBase>().Single();

        // Assert
        concrete.Should().BeSameAs(viaBase);
    }

    [Test]
    public void AddSeeder_DifferentInstancesAcrossScopes()
    {
        // Arrange & Act
        using var provider = BuildProvider(b => b.AddSeeder<UserSeeder>());
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var seeder1 = scope1.ServiceProvider.GetRequiredService<UserSeeder>();
        var seeder2 = scope2.ServiceProvider.GetRequiredService<UserSeeder>();

        // Assert
        seeder1.Should().NotBeSameAs(seeder2);
    }

    [Test]
    public void AddSeeder_ReturnsBuilderForChaining()
    {
        // Arrange
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();
        global::FluentSeeding.DependencyInjection.SeederBuilder? capturedBuilder = null;
        global::FluentSeeding.DependencyInjection.SeederBuilder? returnedBuilder = null;

        // Act
        services.AddFluentSeeding(b =>
        {
            capturedBuilder = b;
            returnedBuilder = b.AddSeeder<UserSeeder>();
        });

        // Assert
        returnedBuilder.Should().BeSameAs(capturedBuilder);
    }
}
