using FluentSeeding.Tests.Common;
using Microsoft.EntityFrameworkCore;

namespace FluentSeeding.EntityFrameworkCore.Tests;

public sealed class TestDbContext : DbContext
{
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Product> Products { get; set; } = null!;

    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }
    
}
