using FluentAssertions;
using FluentSeeding.Tests.Common;

namespace FluentSeeding.Tests.SeedBuilder;

[TestFixture(TestName = "SeedBuilder.UsePool")]
[Category("Unit")]
[Category(nameof(SeedBuilder<>))]
public sealed class SeedBuilderUsePoolTests
{
    [Test]
    public void UsePool_WithoutCount_ProducesOneEntityPerPoolItem()
    {
        var existing = new[]
        {
            new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com" },
            new User { Id = Guid.NewGuid(), Name = "Bob",   Email = "bob@example.com" },
            new User { Id = Guid.NewGuid(), Name = "Carol", Email = "carol@example.com" },
        };

        var results = new SeedBuilder<User>()
            .UsePool(existing)
            .Build().ToList();

        results.Should().HaveCount(3);
    }

    [Test]
    public void UsePool_UsesPoolEntitiesAsBase()
    {
        var existing = new[]
        {
            new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com" },
            new User { Id = Guid.NewGuid(), Name = "Bob",   Email = "bob@example.com" },
        };

        var results = new SeedBuilder<User>()
            .UsePool(existing)
            .Build().ToList();

        results.Should().BeEquivalentTo(existing);
    }

    [Test]
    public void UsePool_RulesAreAppliedOnTopOfPoolEntities()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var existing = new[]
        {
            new User { Id = id1, Name = "Alice", Email = "old@example.com" },
            new User { Id = id2, Name = "Bob",   Email = "old@example.com" },
        };

        var results = new SeedBuilder<User>()
            .UsePool(existing)
            .RuleFor(u => u.Email).UseValue("new@example.com")
            .Build().ToList();

        results.Should().HaveCount(2);
        results[0].Id.Should().Be(id1);
        results[0].Name.Should().Be("Alice");
        results[0].Email.Should().Be("new@example.com");
        results[1].Id.Should().Be(id2);
        results[1].Name.Should().Be("Bob");
        results[1].Email.Should().Be("new@example.com");
    }

    [Test]
    public void UsePool_EmptyPool_ProducesNoEntities()
    {
        var results = new SeedBuilder<User>()
            .UsePool(Array.Empty<User>())
            .Build();

        results.Should().BeEmpty();
    }

    [Test]
    public void UsePool_CountGreaterThanPoolSize_CyclesThroughPool()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var existing = new[]
        {
            new User { Id = id1, Name = "Alice", Email = "alice@example.com" },
            new User { Id = id2, Name = "Bob",   Email = "bob@example.com" },
        };

        var results = new SeedBuilder<User>()
            .UsePool(existing)
            .Count(5)
            .Build().ToList();

        results.Should().HaveCount(5);
        results[0].Id.Should().Be(id1);
        results[1].Id.Should().Be(id2);
        results[2].Id.Should().Be(id1);
        results[3].Id.Should().Be(id2);
        results[4].Id.Should().Be(id1);
    }

    [Test]
    public void UsePool_CountSmallerThanPoolSize_ProducesOnlyCountEntities()
    {
        var existing = new[]
        {
            new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com" },
            new User { Id = Guid.NewGuid(), Name = "Bob",   Email = "bob@example.com" },
            new User { Id = Guid.NewGuid(), Name = "Carol", Email = "carol@example.com" },
        };

        var results = new SeedBuilder<User>()
            .UsePool(existing)
            .Count(2)
            .Build().ToList();

        results.Should().HaveCount(2);
    }

    [Test]
    public void UsePool_WithForEach_UsesPoolEntitiesAsBaseForEachCombination()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var existing = new[]
        {
            new User { Id = id1, Name = "Alice", Email = "old@example.com" },
            new User { Id = id2, Name = "Bob",   Email = "old@example.com" },
        };
        var emails = new[] { "a@example.com", "b@example.com" };

        var results = new SeedBuilder<User>()
            .UsePool(existing)
            .ForEach(emails, (b, email) => b.RuleFor(u => u.Email).UseValue(email))
            .Build().ToList();

        results.Should().HaveCount(2);
        results[0].Id.Should().Be(id1);
        results[0].Name.Should().Be("Alice");
        results[0].Email.Should().Be("a@example.com");
        results[1].Id.Should().Be(id2);
        results[1].Name.Should().Be("Bob");
        results[1].Email.Should().Be("b@example.com");
    }

    [Test]
    public void UsePool_WithForEach_MoreCombinationsThanPool_CyclesThroughPool()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var existing = new[]
        {
            new User { Id = id1, Name = "Alice", Email = "old@example.com" },
            new User { Id = id2, Name = "Bob",   Email = "old@example.com" },
        };
        var emails = new[] { "a@example.com", "b@example.com", "c@example.com" };

        var results = new SeedBuilder<User>()
            .UsePool(existing)
            .ForEach(emails, (b, email) => b.RuleFor(u => u.Email).UseValue(email))
            .Build().ToList();

        results.Should().HaveCount(3);
        results[0].Id.Should().Be(id1);
        results[1].Id.Should().Be(id2);
        results[2].Id.Should().Be(id1);
    }

    [Test]
    public void UsePool_IndexPassedToRulesIncreasesAcrossItems()
    {
        var existing = new[]
        {
            new User { Id = Guid.NewGuid(), Name = "Alice", Email = "alice@example.com" },
            new User { Id = Guid.NewGuid(), Name = "Bob",   Email = "bob@example.com" },
            new User { Id = Guid.NewGuid(), Name = "Carol", Email = "carol@example.com" },
        };

        var capturedIndices = new List<int>();
        var results = new SeedBuilder<User>()
            .UsePool(existing)
            .RuleFor(u => u.Email).UseFactory(i => { capturedIndices.Add(i); return $"user{i}@example.com"; })
            .Build().ToList();

        capturedIndices.Should().ContainInOrder(0, 1, 2);
    }
}
