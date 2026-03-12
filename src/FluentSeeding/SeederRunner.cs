namespace FluentSeeding;

public sealed class SeederRunner
{
    private readonly IPersistenceLayer _persistenceLayer;
    private readonly IEnumerable<EntitySeederBase> _seeders;
    
    public SeederRunner(IPersistenceLayer persistenceLayer, IEnumerable<EntitySeederBase> seeders)
    {
        _persistenceLayer = persistenceLayer;
        _seeders = seeders;
    }
    
    public void Run()
    {
        foreach (var seeder in _seeders)
        {
            seeder.PersistTo(_persistenceLayer);
        }
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        foreach (var seeder in _seeders)
        {
            await seeder.PersistToAsync(_persistenceLayer, cancellationToken);
        }
    }
}
