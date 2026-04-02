using FluentAssertions;
using FluentSeeding.EntityFrameworkCore;
using FluentSeeding.Tests.Common;
using FluentSeeding.Tests.Common.Seeders;

namespace FluentSeeding.EntityFrameworkCore.Tests;

[TestFixture]
[Category("Integration")]
[Category("Relationships")]
public sealed class ConflictBehaviorWithRelationshipsTests
{
    private TestDbContext _dbContext = null!;

    [SetUp]
    public void SetUp()
    {
        _dbContext = SqliteDbContextFactory.CreateInMemoryDbContext();
    }

    [TearDown]
    public void TearDown()
    {
        _dbContext.Dispose();
    }

    [Test]
    public void Persist_WithSkipConflictBehavior_ShouldNotThrowWhenSeedingWithTrackedNavigation()
    {
        // This is the core issue: when seeding a child entity whose navigation property
        // references an already-tracked parent, EF Core would throw "cannot be tracked" error.
        // The fix clears the navigation but preserves scalar FKs.

        // Arrange
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Skip);
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        var author = new Author { Id = authorId, Name = "Existing Author" };
        _dbContext.Authors.Add(author);
        _dbContext.SaveChanges();

        var post = new Post
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Test Post",
            Content = "Content",
            Author = author  // This reference is already tracked
        };

        // Act & Assert - should not throw
        layer.Persist<Post>([post]);
        layer.Flush();

        // Verify FK is preserved
        _dbContext.Posts.Should().HaveCount(1);
        _dbContext.Posts.Single().AuthorId.Should().Be(authorId);
    }

    [Test]
    public void Persist_WithUpdateConflictBehavior_ShouldNotThrowWhenSeedingWithTrackedNavigation()
    {
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Update);
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        var author = new Author { Id = authorId, Name = "Author" };
        _dbContext.Authors.Add(author);
        _dbContext.SaveChanges();

        var post = new Post
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Post",
            Content = "Content",
            Author = author
        };

        // Act & Assert - should not throw
        layer.Persist<Post>([post]);
        layer.Flush();

        _dbContext.Posts.Should().HaveCount(1);
        _dbContext.Posts.Single().AuthorId.Should().Be(authorId);
    }

    [Test]
    public async Task PersistAsync_WithSkipConflictBehavior_ShouldNotThrowWhenSeedingWithTrackedNavigation()
    {
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Skip);
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        var author = new Author { Id = authorId, Name = "Author" };
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();

        var post = new Post
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Post",
            Content = "Content",
            Author = author
        };

        // Act & Assert - should not throw
        await layer.PersistAsync<Post>([post]);
        await layer.FlushAsync();

        _dbContext.Posts.Should().HaveCount(1);
        _dbContext.Posts.Single().AuthorId.Should().Be(authorId);
    }

    [Test]
    public async Task PersistAsync_WithUpdateConflictBehavior_ShouldNotThrowWhenSeedingWithTrackedNavigation()
    {
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Update);
        var authorId = Guid.NewGuid();
        var postId = Guid.NewGuid();

        var author = new Author { Id = authorId, Name = "Author" };
        _dbContext.Authors.Add(author);
        await _dbContext.SaveChangesAsync();

        var post = new Post
        {
            Id = postId,
            AuthorId = authorId,
            Title = "Post",
            Content = "Content",
            Author = author
        };

        // Act & Assert - should not throw
        await layer.PersistAsync<Post>([post]);
        await layer.FlushAsync();

        _dbContext.Posts.Should().HaveCount(1);
        _dbContext.Posts.Single().AuthorId.Should().Be(authorId);
    }

    [Test]
    public void Persist_WithSkipConflictBehavior_ShouldNotThrowWithCollectionNavigation()
    {
        // Test with collection navigation (1-n relationship)
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Skip);
        var authorId = Guid.NewGuid();

        var author = new Author { Id = authorId, Name = "Author" };
        _dbContext.Authors.Add(author);
        _dbContext.SaveChanges();

        var postId = Guid.NewGuid();
        var authorWithPost = new Author
        {
            Id = authorId,
            Name = "Updated",
            Posts =
            [
                new Post { Id = postId, AuthorId = authorId, Title = "Post", Content = "Content" }
            ]
        };

        // Act & Assert - should not throw even with collection navigation
        layer.Persist<Author>([authorWithPost]);
        layer.Flush();

        // Author should be skipped (already exists)
        _dbContext.Authors.Should().HaveCount(1);
        _dbContext.Authors.Single().Name.Should().Be("Author");
    }

    [Test]
    public void Persist_WithMultipleMixedRelationships_ShouldNotThrow()
    {
        // Real-world scenario: seed posts with author references
        var layer = new EntityFrameworkCorePersistenceLayer(_dbContext, ConflictBehavior.Skip);

        var author1 = new Author { Id = Guid.NewGuid(), Name = "Author 1" };
        var author2 = new Author { Id = Guid.NewGuid(), Name = "Author 2" };
        _dbContext.Authors.AddRange(author1, author2);
        _dbContext.SaveChanges();

        var posts = new[]
        {
            new Post
            {
                Id = Guid.NewGuid(),
                AuthorId = author1.Id,
                Title = "Post 1",
                Content = "Content 1",
                Author = author1  // Already tracked
            },
            new Post
            {
                Id = Guid.NewGuid(),
                AuthorId = author2.Id,
                Title = "Post 2",
                Content = "Content 2",
                Author = author2  // Already tracked
            }
        };

        // Act & Assert - should not throw
        layer.Persist<Post>(posts);
        layer.Flush();

        // Verify posts are persisted with correct FKs
        _dbContext.Posts.Should().HaveCount(2);
        var authorIds = new[] { author1.Id, author2.Id };
        _dbContext.Posts.Should().AllSatisfy(p =>
        {
            authorIds.Should().Contain(p.AuthorId);
        });
    }
}
