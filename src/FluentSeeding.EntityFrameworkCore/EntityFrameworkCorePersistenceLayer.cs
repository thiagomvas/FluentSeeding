using Microsoft.EntityFrameworkCore;

namespace FluentSeeding.EntityFrameworkCore;

/// <summary>
/// An <see cref="IPersistenceLayer"/> backed by an EF Core <see cref="DbContext"/>.
/// Stages all entities via <see cref="Persist{T}"/> and commits them in a single
/// <c>SaveChanges</c> call via <see cref="Flush"/>, ensuring the entire seed run is atomic.
/// </summary>
public sealed class EntityFrameworkCorePersistenceLayer : IPersistenceLayer
{
    private readonly DbContext _dbContext;
    private readonly ConflictBehavior _conflictBehavior;

    /// <param name="dbContext">The EF Core context to stage and commit entities against.</param>
    /// <param name="conflictBehavior">
    /// How to handle entities whose primary key already exists in the database.
    /// Defaults to <see cref="ConflictBehavior.Insert"/>.
    /// </param>
    public EntityFrameworkCorePersistenceLayer(DbContext dbContext, ConflictBehavior conflictBehavior = ConflictBehavior.Insert)
    {
        _dbContext = dbContext;
        _conflictBehavior = conflictBehavior;
    }

    /// <inheritdoc />
    public void Persist<T>(IEnumerable<T> entities) where T : class
    {
        switch (_conflictBehavior)
        {
            case ConflictBehavior.Insert:
                _dbContext.Set<T>().AddRange(entities);
                break;

            case ConflictBehavior.Skip:
                foreach (var entity in entities)
                {
                    var keyValues = GetKeyValues(entity);
                    if (_dbContext.Set<T>().Find(keyValues) is null)
                        _dbContext.Set<T>().Add(entity);
                }
                break;

            case ConflictBehavior.Update:
                foreach (var entity in entities)
                {
                    var keyValues = GetKeyValues(entity);
                    var existing = _dbContext.Set<T>().Find(keyValues);
                    if (existing is null)
                        _dbContext.Set<T>().Add(entity);
                    else
                        _dbContext.Entry(existing).CurrentValues.SetValues(entity);
                }
                break;
        }
    }

    /// <inheritdoc />
    public async Task PersistAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class
    {
        switch (_conflictBehavior)
        {
            case ConflictBehavior.Insert:
                _dbContext.Set<T>().AddRange(entities);
                break;

            case ConflictBehavior.Skip:
                foreach (var entity in entities)
                {
                    var keyValues = GetKeyValues(entity);
                    if (await _dbContext.Set<T>().FindAsync(keyValues, cancellationToken) is null)
                        _dbContext.Set<T>().Add(entity);
                }
                break;

            case ConflictBehavior.Update:
                foreach (var entity in entities)
                {
                    var keyValues = GetKeyValues(entity);
                    var existing = await _dbContext.Set<T>().FindAsync(keyValues, cancellationToken);
                    if (existing is null)
                        _dbContext.Set<T>().Add(entity);
                    else
                        _dbContext.Entry(existing).CurrentValues.SetValues(entity);
                }
                break;
        }
    }

    /// <inheritdoc />
    public void Flush()
    {
        _dbContext.SaveChanges();
    }

    /// <inheritdoc />
    public Task FlushAsync(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }

    private object[] GetKeyValues<T>(T entity) where T : class
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException(
                $"Entity type '{typeof(T).Name}' is not registered in the DbContext model.");

        var primaryKey = entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException(
                $"Entity type '{typeof(T).Name}' has no primary key defined.");

        return primaryKey.Properties
            .Select(p => p.PropertyInfo is { } pi
                ? pi.GetValue(entity)!
                : throw new InvalidOperationException(
                    $"Shadow property '{p.Name}' on '{typeof(T).Name}' cannot be used for conflict resolution. Only mapped CLR properties are supported."))
            .ToArray();
    }
}
