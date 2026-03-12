namespace FluentSeeding;

public abstract class EntitySeederBase
{
    internal abstract IEnumerable<object> SeedInternal();
    internal abstract void PersistTo(IPersistenceLayer persistenceLayer);
    internal abstract Task PersistToAsync(IPersistenceLayer persistenceLayer, CancellationToken cancellationToken);
}
