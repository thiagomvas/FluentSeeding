using FluentSeeding.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace FluentSeeding.EntityFrameworkCore.Tests;

public sealed class TestDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;
    public DbSet<Author> Authors { get; set; } = null!;
    public DbSet<AuthorProfile> AuthorProfiles { get; set; } = null!;
    public DbSet<Post> Posts { get; set; } = null!;
    public DbSet<Tag> Tags { get; set; } = null!;
    public DbSet<PostTag> PostTags { get; set; } = null!;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Author-AuthorProfile 1-1 relationship
        modelBuilder.Entity<Author>()
            .HasOne(a => a.Profile)
            .WithOne(p => p.Author)
            .HasForeignKey<AuthorProfile>(p => p.AuthorId);

        // Configure Author-Post 1-n relationship
        modelBuilder.Entity<Author>()
            .HasMany(a => a.Posts)
            .WithOne(p => p.Author)
            .HasForeignKey(p => p.AuthorId);

        // Configure Post-PostTag n-n relationship
        modelBuilder.Entity<Post>()
            .HasMany(p => p.PostTags)
            .WithOne(pt => pt.Post)
            .HasForeignKey(pt => pt.PostId);

        modelBuilder.Entity<Tag>()
            .HasMany(t => t.PostTags)
            .WithOne(pt => pt.Tag)
            .HasForeignKey(pt => pt.TagId);

        // Configure PostTag composite primary key
        modelBuilder.Entity<PostTag>()
            .HasKey(pt => new { pt.PostId, pt.TagId });
    }
}
