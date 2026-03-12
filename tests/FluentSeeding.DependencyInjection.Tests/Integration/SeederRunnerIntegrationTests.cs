using FluentAssertions;
using FluentSeeding.DependencyInjection;
using FluentSeeding.Tests.Common;
using FluentSeeding.Tests.Common.Seeders;
using Microsoft.Extensions.DependencyInjection;

namespace FluentSeeding.DependencyInjection.Tests.Integration;

[TestFixture(TestName = "SeederRunner.Integration")]
[Category("Integration")]
[Category(nameof(SeederRunner))]
public sealed class SeederRunnerIntegrationTests
{
    private ServiceProvider _provider = null!;

    [SetUp]
    public void SetUp()
    {
        var services = new Microsoft.Extensions.DependencyInjection.ServiceCollection();

        services.AddScoped<InMemoryPersistenceLayer>();
        services.AddScoped<IPersistenceLayer>(sp => sp.GetRequiredService<InMemoryPersistenceLayer>());

        services.AddFluentSeeding(b => b
            .AddSeeder<UserSeeder>()
            .AddSeeder<ProductSeeder>()
            .AddSeeder<PurchaseSeeder>());

        _provider = services.BuildServiceProvider();
    }

    [TearDown]
    public void TearDown() => _provider.Dispose();

    [Test]
    public void Run_UserSeeder_PersistsExpectedUserCount()
    {
        // Arrange
        using var scope = _provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<SeederRunner>();
        var store = scope.ServiceProvider.GetRequiredService<InMemoryPersistenceLayer>();

        // Act
        runner.Run();

        // Assert
        store.GetEntities<User>().Should().HaveCount(10);
    }

    [Test]
    public void Run_ProductSeeder_PersistsExpectedProductCount()
    {
        // Arrange
        using var scope = _provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<SeederRunner>();
        var store = scope.ServiceProvider.GetRequiredService<InMemoryPersistenceLayer>();

        // Act
        runner.Run();

        // Assert
        store.GetEntities<Product>().Should().HaveCount(5);
    }

    [Test]
    public void Run_PurchaseSeeder_PersistsPurchases()
    {
        // Arrange
        using var scope = _provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<SeederRunner>();
        var store = scope.ServiceProvider.GetRequiredService<InMemoryPersistenceLayer>();

        // Act
        runner.Run();

        // Assert
        store.GetEntities<Purchase>().Should().NotBeEmpty();
    }

    [Test]
    public void Run_PurchaseSeeder_UserIdsReferenceSeededUsers()
    {
        // Arrange
        using var scope = _provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<SeederRunner>();
        var store = scope.ServiceProvider.GetRequiredService<InMemoryPersistenceLayer>();

        // Act
        runner.Run();

        // Assert
        var userIds = store.GetEntities<User>().Select(u => u.Id).ToHashSet();
        store.GetEntities<Purchase>()
            .Should().AllSatisfy(p => userIds.Should().Contain(p.UserId));
    }

    [Test]
    public void Run_PurchaseSeeder_ProductIdsReferenceSeededProducts()
    {
        // Arrange
        using var scope = _provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<SeederRunner>();
        var store = scope.ServiceProvider.GetRequiredService<InMemoryPersistenceLayer>();

        // Act
        runner.Run();

        // Assert
        var productIds = store.GetEntities<Product>().Select(p => p.Id).ToHashSet();
        store.GetEntities<Purchase>()
            .Should().AllSatisfy(p => productIds.Should().Contain(p.ProductId));
    }

    [Test]
    public async Task RunAsync_UserSeeder_PersistsExpectedUserCount()
    {
        // Arrange
        using var scope = _provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<SeederRunner>();
        var store = scope.ServiceProvider.GetRequiredService<InMemoryPersistenceLayer>();

        // Act
        await runner.RunAsync();

        // Assert
        store.GetEntities<User>().Should().HaveCount(10);
    }

    [Test]
    public async Task RunAsync_PurchaseSeeder_UserIdsReferenceSeededUsers()
    {
        // Arrange
        using var scope = _provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<SeederRunner>();
        var store = scope.ServiceProvider.GetRequiredService<InMemoryPersistenceLayer>();

        // Act
        await runner.RunAsync();

        // Assert
        var userIds = store.GetEntities<User>().Select(u => u.Id).ToHashSet();
        store.GetEntities<Purchase>()
            .Should().AllSatisfy(p => userIds.Should().Contain(p.UserId));
    }

    [Test]
    public async Task RunAsync_PurchaseSeeder_ProductIdsReferenceSeededProducts()
    {
        // Arrange
        using var scope = _provider.CreateScope();
        var runner = scope.ServiceProvider.GetRequiredService<SeederRunner>();
        var store = scope.ServiceProvider.GetRequiredService<InMemoryPersistenceLayer>();

        // Act
        await runner.RunAsync();

        // Assert
        var productIds = store.GetEntities<Product>().Select(p => p.Id).ToHashSet();
        store.GetEntities<Purchase>()
            .Should().AllSatisfy(p => productIds.Should().Contain(p.ProductId));
    }
}
