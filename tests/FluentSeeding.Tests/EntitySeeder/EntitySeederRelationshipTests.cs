using FluentAssertions;
using FluentSeeding.Tests.Common.Seeders;

namespace FluentSeeding.Tests.EntitySeeder;

[TestFixture(TestName = "EntitySeeder.Relationship")]
[Category("Unit")]
[Category(nameof(EntitySeeder<>))]
public sealed class EntitySeederRelationshipTests
{
    private UserSeeder _userSeeder = null!;
    private ProductSeeder _productSeeder = null!;
    private PurchaseSeeder _purchaseSeeder = null!;

    [SetUp]
    public void SetUp()
    {
        _userSeeder = new UserSeeder();
        _productSeeder = new ProductSeeder();
        _purchaseSeeder = new PurchaseSeeder(_userSeeder, _productSeeder);
    }

    [Test]
    public void Seed_WhenRelatedSeedersProvided_AllUserIdsReferenceSeededUsers()
    {
        // Arrange
        var userIds = _userSeeder.Data.Select(u => u.Id).ToHashSet();

        // Act
        var purchases = _purchaseSeeder.Seed().ToList();

        // Assert
        purchases.Should().OnlyContain(p => userIds.Contains(p.UserId));
    }

    [Test]
    public void Seed_WhenRelatedSeedersProvided_AllProductIdsReferenceSeededProducts()
    {
        // Arrange
        var productIds = _productSeeder.Data.Select(p => p.Id).ToHashSet();

        // Act
        var purchases = _purchaseSeeder.Seed().ToList();

        // Assert
        purchases.Should().OnlyContain(p => productIds.Contains(p.ProductId));
    }

    [Test]
    public void Seed_WhenCalled_EachPurchaseHasUniqueId()
    {
        // Arrange & Act
        var purchases = _purchaseSeeder.Seed().ToList();

        // Assert
        purchases.Select(p => p.Id).Should().OnlyHaveUniqueItems();
    }

    [Test]
    public void Seed_WhenCalled_AllIdsAreNonEmpty()
    {
        // Act
        var purchases = _purchaseSeeder.Seed().ToList();

        // Assert
        purchases.Should().OnlyContain(p => p.Id != Guid.Empty);
    }

    [Test]
    public void Seed_WhenCalled_QuantityIsWithinExpectedRange()
    {
        // Act
        var purchases = _purchaseSeeder.Seed().ToList();

        // Assert
        purchases.Should().OnlyContain(p => p.Quantity >= 1 && p.Quantity <= 99);
    }

    [Test]
    public void Data_WhenAccessedMultipleTimes_ReturnsSameCachedInstances()
    {
        // Act
        var first = _purchaseSeeder.Data;
        var second = _purchaseSeeder.Data;

        // Assert
        first.Should().BeSameAs(second);
    }

    [Test]
    public void Seed_WhenCalledMultipleTimes_ReturnsNewInstancesButStillValidRelationships()
    {
        // Arrange
        var userIds = _userSeeder.Data.Select(u => u.Id).ToHashSet();
        var productIds = _productSeeder.Data.Select(p => p.Id).ToHashSet();

        // Act
        var first = _purchaseSeeder.Seed().ToList();
        var second = _purchaseSeeder.Seed().ToList();

        // Assert
        first.Zip(second).Should().OnlyContain(pair => !ReferenceEquals(pair.First, pair.Second));
        second.Should().OnlyContain(p => userIds.Contains(p.UserId) && productIds.Contains(p.ProductId));
    }

    [Test]
    public void Seed_WhenRelatedSeedersHaveDistinctIds_UserAndProductIdsDoNotCrossContaminate()
    {
        // Arrange
        var userIds = _userSeeder.Data.Select(u => u.Id).ToHashSet();
        var productIds = _productSeeder.Data.Select(p => p.Id).ToHashSet();

        // Act
        var purchases = _purchaseSeeder.Seed().ToList();

        // Assert
        purchases.Should().OnlyContain(p => userIds.Contains(p.UserId) && !userIds.Contains(p.ProductId)
                                         || productIds.Contains(p.ProductId) && !productIds.Contains(p.UserId));
    }
}
