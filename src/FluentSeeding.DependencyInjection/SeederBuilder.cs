using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace FluentSeeding.DependencyInjection;

/// <summary>
/// Configures which <see cref="EntitySeederBase"/> implementations are registered in the DI container.
/// Obtain an instance via <see cref="ServiceCollectionExtensions.AddFluentSeeding"/>.
/// </summary>
public sealed class SeederBuilder
{
    private readonly IServiceCollection _services;

    internal SeederBuilder(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Registers a single seeder as a scoped service.
    /// The seeder is resolvable by its concrete type and as <see cref="EntitySeederBase"/>
    /// so it participates in <see cref="SeederRunner"/> orchestration.
    /// </summary>
    /// <typeparam name="TSeeder">Concrete seeder type to register.</typeparam>
    /// <returns>The same builder for chaining.</returns>
    public SeederBuilder AddSeeder<TSeeder>() where TSeeder : EntitySeederBase
    {
        _services.AddScoped<TSeeder>();
        _services.AddScoped<EntitySeederBase>(sp => sp.GetRequiredService<TSeeder>());
        return this;
    }

    /// <summary>
    /// Scans <paramref name="assembly"/> and registers every non-abstract
    /// <see cref="EntitySeederBase"/> subclass found there.
    /// </summary>
    /// <param name="assembly">The assembly to scan.</param>
    /// <returns>The same builder for chaining.</returns>
    public SeederBuilder AddSeedersFromAssembly(Assembly assembly)
    {
        var seederTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsAssignableTo(typeof(EntitySeederBase)));

        foreach (var seederType in seederTypes)
        {
            _services.AddScoped(seederType);
            _services.AddScoped(typeof(EntitySeederBase), sp => sp.GetRequiredService(seederType));
        }

        return this;
    }

    /// <summary>
    /// Scans the assembly that contains <typeparamref name="T"/> and registers every
    /// non-abstract <see cref="EntitySeederBase"/> subclass found there.
    /// </summary>
    /// <typeparam name="T">Any type whose assembly should be scanned.</typeparam>
    /// <returns>The same builder for chaining.</returns>
    public SeederBuilder AddSeedersFromAssemblyContaining<T>()
        => AddSeedersFromAssembly(typeof(T).Assembly);
}
