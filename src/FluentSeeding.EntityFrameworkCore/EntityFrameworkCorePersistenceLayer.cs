using Microsoft.EntityFrameworkCore;

namespace FluentSeeding.EntityFrameworkCore;

internal sealed class EntityFrameworkCorePersistenceLayer : IPersistenceLayer
{
    private readonly DbContext _dbContext;

    public EntityFrameworkCorePersistenceLayer(DbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Persist<T>(IEnumerable<T> entities) where T : class
    {
        _dbContext.Set<T>().AddRange(entities);
        _dbContext.SaveChanges();
    }

    public Task PersistAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class
    {
        _dbContext.Set<T>().AddRange(entities);
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
