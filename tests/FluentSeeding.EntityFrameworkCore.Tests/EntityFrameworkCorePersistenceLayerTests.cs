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

    [Test]
    public void Persist_WithSkipConflictBehavior_ShouldNotInsertEntityWhenKeyAlreadyExists()
    {
        // Arrange
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Skip);
        var id = Guid.NewGuid();
        _dbContext.Products.Add(new Product { Id = id, Name = "Original", Price = 1.00m });
        _dbContext.SaveChanges();

        // Act
        layer.Persist<Product>([new Product { Id = id, Name = "New", Price = 99.00m }]);
        layer.Flush();

        // Assert
        _dbContext.Products.Should().HaveCount(1);
        _dbContext.Products.Single().Name.Should().Be("Original");
    }

    [Test]
    public void Persist_WithSkipConflictBehavior_ShouldInsertEntityWhenKeyDoesNotExist()
    {
        // Arrange
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Skip);

        // Act
        layer.Persist<Product>([new Product { Id = Guid.NewGuid(), Name = "New", Price = 9.99m }]);
        layer.Flush();

        // Assert
        _dbContext.Products.Should().HaveCount(1);
    }

    [Test]
    public void Persist_WithSkipConflictBehavior_ShouldOnlyInsertNewEntitiesWhenMixed()
    {
        // Arrange
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Skip);
        var existingId = Guid.NewGuid();
        _dbContext.Products.Add(new Product { Id = existingId, Name = "Existing", Price = 1.00m });
        _dbContext.SaveChanges();

        var newId = Guid.NewGuid();
        Product[] toSeed =
        [
            new Product { Id = existingId, Name = "ShouldBeSkipped", Price = 0.00m },
            new Product { Id = newId, Name = "ShouldBeInserted", Price = 5.00m },
        ];

        // Act
        layer.Persist<Product>(toSeed);
        layer.Flush();

        // Assert
        _dbContext.Products.Should().HaveCount(2);
        _dbContext.Products.Single(p => p.Id == existingId).Name.Should().Be("Existing");
        _dbContext.Products.Single(p => p.Id == newId).Name.Should().Be("ShouldBeInserted");
    }

    [Test]
    public void Persist_WithUpdateConflictBehavior_ShouldUpdateEntityWhenKeyAlreadyExists()
    {
        // Arrange
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Update);
        var id = Guid.NewGuid();
        _dbContext.Products.Add(new Product { Id = id, Name = "Original", Price = 1.00m });
        _dbContext.SaveChanges();

        // Act
        layer.Persist<Product>([new Product { Id = id, Name = "Updated", Price = 49.99m }]);
        layer.Flush();

        // Assert
        _dbContext.Products.Should().HaveCount(1);
        var product = _dbContext.Products.Single();
        product.Name.Should().Be("Updated");
        product.Price.Should().Be(49.99m);
    }

    [Test]
    public void Persist_WithUpdateConflictBehavior_ShouldInsertEntityWhenKeyDoesNotExist()
    {
        // Arrange
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Update);

        // Act
        layer.Persist<Product>([new Product { Id = Guid.NewGuid(), Name = "New", Price = 9.99m }]);
        layer.Flush();

        // Assert
        _dbContext.Products.Should().HaveCount(1);
    }

    [Test]
    public void Persist_WithUpdateConflictBehavior_ShouldUpdateExistingAndInsertNewEntitiesWhenMixed()
    {
        // Arrange
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Update);
        var existingId = Guid.NewGuid();
        _dbContext.Products.Add(new Product { Id = existingId, Name = "Original", Price = 1.00m });
        _dbContext.SaveChanges();

        var newId = Guid.NewGuid();
        Product[] toSeed =
        [
            new Product { Id = existingId, Name = "Updated", Price = 49.99m },
            new Product { Id = newId, Name = "Inserted", Price = 5.00m },
        ];

        // Act
        layer.Persist<Product>(toSeed);
        layer.Flush();

        // Assert
        _dbContext.Products.Should().HaveCount(2);
        _dbContext.Products.Single(p => p.Id == existingId).Name.Should().Be("Updated");
        _dbContext.Products.Single(p => p.Id == newId).Name.Should().Be("Inserted");
    }

    [Test]
    public async Task PersistAsync_WithSkipConflictBehavior_ShouldNotInsertEntityWhenKeyAlreadyExists()
    {
        // Arrange
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Skip);
        var id = Guid.NewGuid();
        _dbContext.Products.Add(new Product { Id = id, Name = "Original", Price = 1.00m });
        await _dbContext.SaveChangesAsync();

        // Act
        await layer.PersistAsync<Product>([new Product { Id = id, Name = "New", Price = 99.00m }]);
        await layer.FlushAsync();

        // Assert
        _dbContext.Products.Should().HaveCount(1);
        _dbContext.Products.Single().Name.Should().Be("Original");
    }

    [Test]
    public async Task PersistAsync_WithUpdateConflictBehavior_ShouldUpdateEntityWhenKeyAlreadyExists()
    {
        // Arrange
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Update);
        var id = Guid.NewGuid();
        _dbContext.Products.Add(new Product { Id = id, Name = "Original", Price = 1.00m });
        await _dbContext.SaveChangesAsync();

        // Act
        await layer.PersistAsync<Product>([new Product { Id = id, Name = "Updated", Price = 49.99m }]);
        await layer.FlushAsync();

        // Assert
        _dbContext.Products.Should().HaveCount(1);
        var product = _dbContext.Products.Single();
        product.Name.Should().Be("Updated");
        product.Price.Should().Be(49.99m);
    }
}
