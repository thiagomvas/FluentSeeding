using Microsoft.Extensions.DependencyInjection;

namespace FluentSeeding.DependencyInjection;

/// <summary>
/// Extension methods for registering FluentSeeding services in an <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds FluentSeeding to the service collection.
    /// Registers a scoped <see cref="SeederRunner"/> and applies seeder registrations via <paramref name="configure"/>.
    /// </summary>
    /// <remarks>
    /// An <see cref="IPersistenceLayer"/> must be registered separately before resolving <see cref="SeederRunner"/>.
    /// </remarks>
    /// <param name="services">The service collection to add to.</param>
    /// <param name="configure">Delegate that adds seeders via <see cref="SeederBuilder"/>.</param>
    /// <returns>The same <see cref="IServiceCollection"/> for chaining.</returns>
    public static IServiceCollection AddFluentSeeding(
        this IServiceCollection services,
        Action<SeederBuilder> configure)
    {
        var builder = new SeederBuilder(services);
        configure(builder);
        services.AddScoped<SeederRunner>();
        return services;
    }
}
