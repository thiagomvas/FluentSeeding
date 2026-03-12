namespace FluentSeeding.Tests.Common;

public sealed class InMemoryPersistenceLayer : IPersistenceLayer
{
    private readonly Dictionary<Type, List<object>> _storage = new();
    public void Persist<T>(IEnumerable<T> entities) where T : class
    {
        var type = typeof(T);
        if (!_storage.ContainsKey(type))
        {
            _storage[type] = new List<object>();
        }
        _storage[type].AddRange(entities.Cast<object>());
        
    }

    public Task PersistAsync<T>(IEnumerable<T> entities, CancellationToken cancellationToken = default) where T : class
    {
        Persist(entities);
        return Task.CompletedTask;
    }
    
    public IReadOnlyList<T> GetEntities<T>()
    {
        var type = typeof(T);
        if (_storage.TryGetValue(type, out var entities))
        {
            return entities.Cast<T>().ToList();
        }
        return new List<T>();
    }
}
