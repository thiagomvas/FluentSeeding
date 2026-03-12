using System.Linq.Expressions;

namespace FluentSeeding;

/// <summary>
/// Fluent builder for configuring how a set of seed entities of type <typeparamref name="T"/> are constructed and populated.
/// </summary>
/// <typeparam name="T">The entity type to seed. Must be a reference type.</typeparam>
public sealed class SeedBuilder<T> where T : class
{
    private readonly List<ISeedRule<T>> _rules = new();
    private Func<T>? _factory;
    private int _countMin = 1;
    private int _countMax = 1;

    /// <summary>
    /// Seeds exactly <paramref name="count"/> entities.
    /// </summary>
    /// <param name="count">The number of entities to generate.</param>
    public SeedBuilder<T> Count(int count)
    {
        _countMin = count;
        _countMax = count;
        return this;
    }

    /// <summary>
    /// Seeds a random number of entities between <paramref name="min"/> and <paramref name="max"/> (inclusive).
    /// </summary>
    /// <param name="min">The minimum number of entities to generate. Must be non-negative.</param>
    /// <param name="max">The maximum number of entities to generate. Must be greater than or equal to <paramref name="min"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when <paramref name="min"/> is negative or <paramref name="max"/> is less than <paramref name="min"/>.
    /// </exception>
    public SeedBuilder<T> Count(int min, int max)
    {
        if (min < 0) throw new ArgumentOutOfRangeException(nameof(min), "Minimum count cannot be negative.");
        if (max < min) throw new ArgumentOutOfRangeException(nameof(max), "Maximum count cannot be less than minimum count.");

        _countMin = min;
        _countMax = max;
        return this;
    }

    /// <summary>
    /// Starts a rule for the property identified by <paramref name="selector"/>.
    /// Chain <c>UseValue</c>, <c>UseFactory</c>, or <c>UseFrom</c> on the returned <see cref="SeedRule{T,TProperty}"/>
    /// to supply the value, which returns this builder for further configuration.
    /// </summary>
    /// <param name="selector">A member expression identifying the property to configure (e.g. <c>x => x.Name</c>).</param>
    /// <returns>A <see cref="SeedRule{T,TProperty}"/> for configuring how the property is populated.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="selector"/> refers to a nested property (e.g. <c>x => x.Address.City</c>).
    /// Only direct properties of <typeparamref name="T"/> are supported.
    /// </exception>
    public SeedRule<T, TProperty> RuleFor<TProperty>(Expression<Func<T, TProperty>> selector)
    {
        if (selector.Body is not MemberExpression { Expression: ParameterExpression })
            throw new ArgumentException(
                $"Selector must target a direct property of {typeof(T).Name}. Nested property access is not supported.",
                nameof(selector));

        var rule = new SeedRule<T, TProperty>(selector, this);
        _rules.Add(rule);
        return rule;
    }

    /// <summary>
    /// Overrides the default <see cref="Activator.CreateInstance{T}"/> with a custom factory delegate.
    /// Useful when <typeparamref name="T"/> has no parameterless constructor or requires specific initialisation.
    /// </summary>
    /// <param name="factory">A delegate that returns a new instance of <typeparamref name="T"/>.</param>
    public SeedBuilder<T> WithFactory(Func<T> factory)
    {
        _factory = factory;
        return this;
    }

    /// <summary>
    /// Materialises the configured entities by invoking the factory (or <see cref="Activator.CreateInstance{T}"/>)
    /// and applying all registered rules to each instance.
    /// </summary>
    /// <returns>
    /// A collection of seeded <typeparamref name="T"/> instances. The count is randomly chosen between
    /// the configured min and max values (inclusive).
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a registered rule has no value or factory configured.
    /// See <see cref="SeedRule{T,TProperty}.Apply"/>.
    /// </exception>
    public IEnumerable<T> Build()
    {
        var random = new Random();
        var count = random.Next(_countMin, _countMax + 1);
        var entities = new List<T>();

        for (int i = 0; i < count; i++)
        {
            var entity = _factory != null ? _factory() : Activator.CreateInstance<T>()!;
            foreach (var rule in _rules)
            {
                rule.Apply(entity, i);
            }
            entities.Add(entity);
        }

        return entities;
    }
}
