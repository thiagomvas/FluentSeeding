using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FluentSeeding.EntityFrameworkCore;

/// <summary>
/// Extension methods for registering the EF Core <see cref="IPersistenceLayer"/> in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers an EF Core-backed <see cref="IPersistenceLayer"/> as a scoped service,
    /// resolving <typeparamref name="TContext"/> from the container.
    /// </summary>
    /// <typeparam name="TContext">
    /// The <see cref="DbContext"/> type to use. Must already be registered in the container
    /// (e.g. via <c>AddDbContext&lt;TContext&gt;</c>).
    /// </typeparam>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">
    /// Optional delegate to configure <see cref="EntityFrameworkCoreSeedingOptions"/>.
    /// When omitted, defaults are used (<see cref="ConflictBehavior.Insert"/>).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFluentSeedingEntityFrameworkCore<TContext>(
        this IServiceCollection services,
        Action<EntityFrameworkCoreSeedingOptions>? configure = null)
        where TContext : DbContext
    {
        var options = new EntityFrameworkCoreSeedingOptions();
        configure?.Invoke(options);

        services.AddScoped<IPersistenceLayer>(sp =>
            new EntityFrameworkCorePersistenceLayer(
                sp.GetRequiredService<TContext>(),
                options.ConflictBehavior));

        return services;
    }

    /// <summary>
    /// Registers an EF Core-backed <see cref="IPersistenceLayer"/> as a scoped service,
    /// resolving a <see cref="DbContext"/> from the container.
    /// Use this overload when the base <see cref="DbContext"/> type is registered directly
    /// rather than a typed subclass.
    /// </summary>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">
    /// Optional delegate to configure <see cref="EntityFrameworkCoreSeedingOptions"/>.
    /// When omitted, defaults are used (<see cref="ConflictBehavior.Insert"/>).
    /// </param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFluentSeedingEntityFrameworkCore(
        this IServiceCollection services,
        Action<EntityFrameworkCoreSeedingOptions>? configure = null)
    {
        var options = new EntityFrameworkCoreSeedingOptions();
        configure?.Invoke(options);

        services.AddScoped<IPersistenceLayer>(sp =>
            new EntityFrameworkCorePersistenceLayer(
                sp.GetRequiredService<DbContext>(),
                options.ConflictBehavior));

        return services;
    }
}
