using FluentAssertions;
using FluentSeeding.EntityFrameworkCore;
using FluentSeeding.Tests.Common;
using FluentSeeding.Tests.Common.Seeders;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentSeeding.EntityFrameworkCore.Tests;

[TestFixture]
[Category("Unit")]
[Category(nameof(ServiceCollectionExtensions))]
public sealed class ServiceCollectionExtensionsTests
{
    private static IServiceCollection BuildServicesWithTypedContext(Action<EntityFrameworkCoreSeedingOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddScoped<TestDbContext>(_ => SqliteDbContextFactory.CreateInMemoryDbContext());
        services.AddFluentSeedingEntityFrameworkCore<TestDbContext>(configure);
        return services;
    }

    private static IServiceCollection BuildServicesWithBaseContext(Action<EntityFrameworkCoreSeedingOptions>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddScoped<DbContext>(_ => SqliteDbContextFactory.CreateInMemoryDbContext());
        services.AddFluentSeedingEntityFrameworkCore(configure);
        return services;
    }

    [Test]
    public void AddFluentSeedingEntityFrameworkCore_TypedContext_RegistersIPersistenceLayer()
    {
        // Arrange
        var services = BuildServicesWithTypedContext();

        // Act
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var layer = scope.ServiceProvider.GetService<IPersistenceLayer>();

        // Assert
        layer.Should().NotBeNull();
        layer.Should().BeOfType<EntityFrameworkCorePersistenceLayer>();
    }

    [Test]
    public void AddFluentSeedingEntityFrameworkCore_TypedContext_IsScoped()
    {
        // Arrange
        var services = BuildServicesWithTypedContext();

        // Act
        using var provider = services.BuildServiceProvider();
        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();
        var layer1a = scope1.ServiceProvider.GetRequiredService<IPersistenceLayer>();
        var layer1b = scope1.ServiceProvider.GetRequiredService<IPersistenceLayer>();
        var layer2 = scope2.ServiceProvider.GetRequiredService<IPersistenceLayer>();

        // Assert
        layer1a.Should().BeSameAs(layer1b);
        layer1a.Should().NotBeSameAs(layer2);
    }

    [Test]
    public void AddFluentSeedingEntityFrameworkCore_TypedContext_ReturnsServicesForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestDbContext>(_ => SqliteDbContextFactory.CreateInMemoryDbContext());

        // Act
        var returned = services.AddFluentSeedingEntityFrameworkCore<TestDbContext>();

        // Assert
        returned.Should().BeSameAs(services);
    }

    [Test]
    public void AddFluentSeedingEntityFrameworkCore_TypedContext_InvokesConfigureAction()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<TestDbContext>(_ => SqliteDbContextFactory.CreateInMemoryDbContext());
        var configureInvoked = false;

        // Act
        services.AddFluentSeedingEntityFrameworkCore<TestDbContext>(_ => configureInvoked = true);

        // Assert
        configureInvoked.Should().BeTrue();
    }

    [Test]
    public void AddFluentSeedingEntityFrameworkCore_TypedContext_NullConfigure_UsesDefaults()
    {
        // Arrange / Act
        var services = BuildServicesWithTypedContext(configure: null);

        // Assert — no exception, layer resolves successfully
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        scope.ServiceProvider.GetRequiredService<IPersistenceLayer>()
            .Should().BeOfType<EntityFrameworkCorePersistenceLayer>();
    }

    [Test]
    public void AddFluentSeedingEntityFrameworkCore_BaseContext_RegistersIPersistenceLayer()
    {
        // Arrange
        var services = BuildServicesWithBaseContext();

        // Act
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var layer = scope.ServiceProvider.GetService<IPersistenceLayer>();

        // Assert
        layer.Should().NotBeNull();
        layer.Should().BeOfType<EntityFrameworkCorePersistenceLayer>();
    }

    [Test]
    public void AddFluentSeedingEntityFrameworkCore_BaseContext_ReturnsServicesForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<DbContext>(_ => SqliteDbContextFactory.CreateInMemoryDbContext());

        // Act
        var returned = services.AddFluentSeedingEntityFrameworkCore();

        // Assert
        returned.Should().BeSameAs(services);
    }

    [Test]
    public void AddFluentSeedingEntityFrameworkCore_BaseContext_InvokesConfigureAction()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddScoped<DbContext>(_ => SqliteDbContextFactory.CreateInMemoryDbContext());
        var configureInvoked = false;

        // Act
        services.AddFluentSeedingEntityFrameworkCore(_ => configureInvoked = true);

        // Assert
        configureInvoked.Should().BeTrue();
    }

    [Test]
    public void AddFluentSeedingEntityFrameworkCore_IntegratesWithSeederRunner()
    {
        // Arrange
        var services = BuildServicesWithTypedContext();
        services.AddScoped<EntitySeederBase>(_ => new ProductSeeder());
        services.AddScoped<SeederRunner>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<SeederRunner>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act
        runner.Run();

        // Assert
        dbContext.Products.Should().HaveCount(5);
    }

    [Test]
    public void AddFluentSeedingEntityFrameworkCore_WithSkipConflictBehavior_SkipsDuplicatesOnReRun()
    {
        // Arrange
        var services = BuildServicesWithTypedContext(o => o.ConflictBehavior = ConflictBehavior.Skip);
        services.AddScoped<EntitySeederBase>(_ => new ProductSeeder());
        services.AddScoped<SeederRunner>();

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        var runner = scope.ServiceProvider.GetRequiredService<SeederRunner>();
        var dbContext = scope.ServiceProvider.GetRequiredService<TestDbContext>();

        // Act — run twice; seeder caches Data so the same 5 products (same GUIDs) are re-presented
        runner.Run();
        runner.Run();

        // Assert — Skip behavior means second run inserts nothing; count stays at 5
        dbContext.Products.Should().HaveCount(5);
    }
}
