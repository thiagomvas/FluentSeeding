namespace FluentSeeding;

public abstract class EntitySeeder<T> : EntitySeederBase
    where T : class
{
    private List<T>? _seedData;
    public IReadOnlyList<T> Data => _seedData ??= Seed().ToList();

    protected abstract void Configure(SeedBuilder<T> builder);

    public IEnumerable<T> Seed()
    {
        var builder = new SeedBuilder<T>();
        Configure(builder);
        return builder.Build();
    }

    internal sealed override IEnumerable<object> SeedInternal()
    {
        return Seed();
    }

    internal override void PersistTo(IPersistenceLayer persistenceLayer)
    {
        persistenceLayer.Persist(Data);
    }

    internal override Task PersistToAsync(IPersistenceLayer persistenceLayer, CancellationToken cancellationToken)
    {
        return persistenceLayer.PersistAsync(Data, cancellationToken);
    }
}
