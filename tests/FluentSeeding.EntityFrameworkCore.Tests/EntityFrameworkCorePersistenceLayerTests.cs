using FluentAssertions;
using FluentSeeding.EntityFrameworkCore;
using FluentSeeding.Tests.Common;
using FluentSeeding.Tests.Common.Seeders;

namespace FluentSeeding.EntityFrameworkCore.Tests;

[TestFixture]
[Category("Integration")]
[Category(nameof(EntityFrameworkCorePersistenceLayer))]
public sealed class EntityFrameworkCorePersistenceLayerTests
{
    private TestDbContext _dbContext = null!;
    private EntityFrameworkCorePersistenceLayer _persistenceLayer = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContext = SqliteDbContextFactory.CreateInMemoryDbContext();
        _persistenceLayer = new EntityFrameworkCorePersistenceLayer(_dbContext);
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public void Run_WithProductSeeder_ShouldPersistProductsToDatabase()
    {
        // Arrange
        EntitySeederBase[] seeders = [new ProductSeeder()];
        var runner = new SeederRunner(_persistenceLayer, seeders);

        // Act
        runner.Run();

        // Assert
        _dbContext.Products.Should().HaveCount(5);
    }

    [Test]
    public void Run_WithProductSeeder_ShouldPersistCorrectProductValues()
    {
        // Arrange
        EntitySeederBase[] seeders = [new ProductSeeder()];
        var runner = new SeederRunner(_persistenceLayer, seeders);

        // Act
        runner.Run();

        // Assert
        _dbContext.Products.Should().AllSatisfy(p =>
        {
            p.Name.Should().Be("Sample Product");
            p.Price.Should().Be(9.99m);
        });
    }

    [Test]
    public void Run_WithUserSeeder_ShouldPersistUsersToDatabase()
    {
        // Arrange
        EntitySeederBase[] seeders = [new UserSeeder()];
        var runner = new SeederRunner(_persistenceLayer, seeders);

        // Act
        runner.Run();

        // Assert
        _dbContext.Users.Should().HaveCount(10);
    }

    [Test]
    public void Run_WithMultipleSeeders_ShouldPersistAllEntitiesToDatabase()
    {
        // Arrange
        EntitySeederBase[] seeders = [new UserSeeder(), new ProductSeeder()];
        var runner = new SeederRunner(_persistenceLayer, seeders);

        // Act
        runner.Run();

        // Assert
        _dbContext.Users.Should().HaveCount(10);
        _dbContext.Products.Should().HaveCount(5);
    }

    [Test]
    public async Task RunAsync_WithProductSeeder_ShouldPersistProductsToDatabase()
    {
        // Arrange
        EntitySeederBase[] seeders = [new ProductSeeder()];
        var runner = new SeederRunner(_persistenceLayer, seeders);

        // Act
        await runner.RunAsync();

        // Assert
        _dbContext.Products.Should().HaveCount(5);
    }

    [Test]
    public async Task RunAsync_WithUserSeeder_ShouldPersistUsersToDatabase()
    {
        // Arrange
        EntitySeederBase[] seeders = [new UserSeeder()];
        var runner = new SeederRunner(_persistenceLayer, seeders);

        // Act
        await runner.RunAsync();

        // Assert
        _dbContext.Users.Should().HaveCount(10);
    }

    [Test]
    public async Task RunAsync_WithMultipleSeeders_ShouldPersistAllEntitiesToDatabase()
    {
        // Arrange
        EntitySeederBase[] seeders = [new UserSeeder(), new ProductSeeder()];
        var runner = new SeederRunner(_persistenceLayer, seeders);

        // Act
        await runner.RunAsync();

        // Assert
        _dbContext.Users.Should().HaveCount(10);
        _dbContext.Products.Should().HaveCount(5);
    }

    [Test]
    public async Task RunAsync_WithCancellationToken_ShouldStillPersistEntities()
    {
        // Arrange
        EntitySeederBase[] seeders = [new ProductSeeder()];
        var runner = new SeederRunner(_persistenceLayer, seeders);
        using var cts = new CancellationTokenSource();

        // Act
        await runner.RunAsync(cts.Token);

        // Assert
        _dbContext.Products.Should().HaveCount(5);
    }
}
