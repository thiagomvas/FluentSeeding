namespace FluentSeeding;

public interface IPersistenceLayer
{
    void Persist<T>(IEnumerable<T> entities) where T : class;
    Task PersistAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class;
}