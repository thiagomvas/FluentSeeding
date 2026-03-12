namespace FluentSeeding;

public abstract class EntitySeeder<T> : EntitySeederBase
    where T : class
{
    protected abstract void Configure(SeedBuilder<T> builder);

    public IEnumerable<T> Seed()
    {
        var builder = new SeedBuilder<T>();
        Configure(builder);
        return builder.Build();
    }
    
    internal override IEnumerable<object> SeedInternal()
    {
        return Seed();
    }

    internal override void PersistTo(IPersistenceLayer persistenceLayer)
    {
        persistenceLayer.Persist(Seed());
    }

    internal override Task PersistToAsync(IPersistenceLayer persistenceLayer, CancellationToken cancellationToken)
    {
        return persistenceLayer.PersistAsync(Seed(), cancellationToken);
    }
}
