using System.Linq.Expressions;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

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
                var skipKey = GetKeyProperties<T>();
                foreach (var entity in entities)
                {
                    if (FindEntity(skipKey, entity) is null)
                        _dbContext.Set<T>().Add(entity);
                }
                break;

            case ConflictBehavior.Update:
                var updateKey = GetKeyProperties<T>();
                foreach (var entity in entities)
                {
                    var existing = FindEntity(updateKey, entity);
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
                var skipKey = GetKeyProperties<T>();
                foreach (var entity in entities)
                {
                    if (await FindEntityAsync<T>(skipKey, entity, cancellationToken) is null)
                        _dbContext.Set<T>().Add(entity);
                }
                break;

            case ConflictBehavior.Update:
                var updateKey = GetKeyProperties<T>();
                foreach (var entity in entities)
                {
                    var existing = await FindEntityAsync<T>(updateKey, entity, cancellationToken);
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

    /// <summary>
    /// Checks the change tracker first (catches entities staged earlier in the same run,
    /// including those in Added state), then falls back to a database query that explicitly
    /// ignores global query filters so that tenant/soft-delete filters cannot hide
    /// already-persisted rows from conflict detection.
    /// </summary>
    private T? FindEntity<T>(IReadOnlyList<IProperty> keyProperties, T entity) where T : class
    {
        return FindTracked<T>(keyProperties, entity)
            ?? _dbContext.Set<T>().IgnoreQueryFilters()
                .FirstOrDefault(BuildKeyPredicate<T>(keyProperties, entity));
    }

    private async Task<T?> FindEntityAsync<T>(
        IReadOnlyList<IProperty> keyProperties,
        T entity,
        CancellationToken cancellationToken) where T : class
    {
        return FindTracked<T>(keyProperties, entity)
            ?? await _dbContext.Set<T>().IgnoreQueryFilters()
                .FirstOrDefaultAsync(BuildKeyPredicate<T>(keyProperties, entity), cancellationToken);
    }

    /// <summary>
    /// Searches the local change tracker by primary key, returning any non-deleted tracked
    /// entity with matching key values. This covers entities staged earlier in the same
    /// seed run (e.g. Added state) before SaveChanges has been called.
    /// </summary>
    private T? FindTracked<T>(IReadOnlyList<IProperty> keyProperties, T entity) where T : class
    {
        return _dbContext.ChangeTracker.Entries<T>()
            .Where(e => e.State != EntityState.Deleted)
            .FirstOrDefault(entry => keyProperties.All(prop =>
                Equals(prop.PropertyInfo!.GetValue(entity), prop.PropertyInfo.GetValue(entry.Entity))))
            ?.Entity;
    }

    private IReadOnlyList<IProperty> GetKeyProperties<T>() where T : class
    {
        var entityType = _dbContext.Model.FindEntityType(typeof(T))
            ?? throw new InvalidOperationException(
                $"Entity type '{typeof(T).Name}' is not registered in the DbContext model.");

        return (entityType.FindPrimaryKey()
            ?? throw new InvalidOperationException(
                $"Entity type '{typeof(T).Name}' has no primary key defined."))
            .Properties;
    }

    private static Expression<Func<T, bool>> BuildKeyPredicate<T>(
        IReadOnlyList<IProperty> keyProperties,
        T entity) where T : class
    {
        var param = Expression.Parameter(typeof(T), "e");
        Expression? body = null;

        foreach (var prop in keyProperties)
        {
            if (prop.PropertyInfo is not { } pi)
                throw new InvalidOperationException(
                    $"Shadow property '{prop.Name}' on '{typeof(T).Name}' cannot be used for conflict resolution. Only mapped CLR properties are supported.");

            var value = pi.GetValue(entity);
            var propAccess = Expression.Property(param, pi);
            var constant = Expression.Constant(value, prop.ClrType);
            var equal = Expression.Equal(propAccess, constant);
            body = body is null ? equal : Expression.AndAlso(body, equal);
        }

        return Expression.Lambda<Func<T, bool>>(body!, param);
    }
}
