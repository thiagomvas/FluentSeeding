using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FluentSeeding.EntityFrameworkCore.Tests;

public sealed class SqliteDbContextFactory
{
    public static TestDbContext CreateInMemoryDbContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseSqlite(connection)
            .Options;

        var context = new TestDbContext(options);
        context.Database.EnsureCreated();

        return context;
    }
}
