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
    private bool _countExplicitlySet;
    private int _nextIndex = 0;
    private List<List<Action<SeedBuilder<T>>>>? _forEachDimensions;
    private bool _forEachUsed;
    private IEnumerable<T>? _baseEntities;

    /// <summary>
    /// Seeds exactly <paramref name="count"/> entities.
    /// </summary>
    /// <param name="count">The number of entities to generate.</param>
    public SeedBuilder<T> Count(int count)
    {
        _countMin = count;
        _countMax = count;
        _countExplicitlySet = true;
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
        _countExplicitlySet = true;
        return this;
    }

    /// <summary>
    /// Configures the builder to use a pool of pre-existing entities as bases instead of creating new instances.
    /// Rules are still applied on top of each pooled entity. When <see cref="Count(int)"/> is not called,
    /// the number of entities produced equals the pool size. When <c>Count</c> exceeds the pool size,
    /// the pool is cycled (round-robin). An empty pool produces no entities.
    /// </summary>
    /// <param name="pool">The pre-existing entities to use as bases.</param>
    public SeedBuilder<T> UsePool(params IEnumerable<T> pool)
    {
        _baseEntities = pool;
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
    /// Configures a single nested object property by building it with a dedicated <see cref="SeedBuilder{TProperty}"/>.
    /// Always produces exactly one nested instance per parent entity, regardless of any <c>Count</c> call
    /// inside <paramref name="configure"/>.
    /// </summary>
    /// <typeparam name="TProperty">The type of the nested object. Must be a reference type.</typeparam>
    /// <param name="selector">A member expression identifying the direct property to populate (e.g. <c>x => x.Address</c>).</param>
    /// <param name="configure">A delegate that configures the nested <see cref="SeedBuilder{TProperty}"/>.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="selector"/> refers to a nested property.
    /// Only direct properties of <typeparamref name="T"/> are supported.
    /// </exception>
    public SeedBuilder<T> HasOne<TProperty>(
        Expression<Func<T, TProperty>> selector,
        Action<SeedBuilder<TProperty>> configure)
        where TProperty : class
    {
        var nested = new SeedBuilder<TProperty>();
        configure(nested);
        nested.Count(1);
        RuleFor(selector).UseFactory(() => nested.Build().Single());
        return this;
    }

    /// <summary>
    /// Configures a collection property typed as <see cref="IEnumerable{TItem}"/> by building its items
    /// with a dedicated <see cref="SeedBuilder{TItem}"/>. The number of items produced is controlled by
    /// <c>Count</c> inside <paramref name="configure"/> (defaults to 1).
    /// </summary>
    /// <typeparam name="TItem">The element type of the collection. Must be a reference type.</typeparam>
    /// <param name="selector">A member expression identifying the direct collection property to populate.</param>
    /// <param name="configure">A delegate that configures the nested <see cref="SeedBuilder{TItem}"/>.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="selector"/> refers to a nested property.
    /// Only direct properties of <typeparamref name="T"/> are supported.
    /// </exception>
    public SeedBuilder<T> HasMany<TItem>(
        Expression<Func<T, IEnumerable<TItem>>> selector,
        Action<SeedBuilder<TItem>> configure)
        where TItem : class
    {
        var nested = new SeedBuilder<TItem>();
        configure(nested);
        RuleFor(selector).UseFactory(() => nested.Build());
        return this;
    }

    /// <summary>
    /// Configures a collection property typed as <see cref="ICollection{TItem}"/> by building its items
    /// with a dedicated <see cref="SeedBuilder{TItem}"/>. The number of items produced is controlled by
    /// <c>Count</c> inside <paramref name="configure"/> (defaults to 1).
    /// </summary>
    /// <typeparam name="TItem">The element type of the collection. Must be a reference type.</typeparam>
    /// <param name="selector">A member expression identifying the direct collection property to populate.</param>
    /// <param name="configure">A delegate that configures the nested <see cref="SeedBuilder{TItem}"/>.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="selector"/> refers to a nested property.
    /// Only direct properties of <typeparamref name="T"/> are supported.
    /// </exception>
    public SeedBuilder<T> HasMany<TItem>(
        Expression<Func<T, ICollection<TItem>>> selector,
        Action<SeedBuilder<TItem>> configure)
        where TItem : class
    {
        var nested = new SeedBuilder<TItem>();
        configure(nested);
        RuleFor(selector).UseFactory(() => nested.Build().ToList());
        return this;
    }

    /// <summary>
    /// Configures a collection property typed as <see cref="List{TItem}"/> by building its items
    /// with a dedicated <see cref="SeedBuilder{TItem}"/>. The number of items produced is controlled by
    /// <c>Count</c> inside <paramref name="configure"/> (defaults to 1).
    /// </summary>
    /// <typeparam name="TItem">The element type of the collection. Must be a reference type.</typeparam>
    /// <param name="selector">A member expression identifying the direct collection property to populate.</param>
    /// <param name="configure">A delegate that configures the nested <see cref="SeedBuilder{TItem}"/>.</param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="selector"/> refers to a nested property.
    /// Only direct properties of <typeparamref name="T"/> are supported.
    /// </exception>
    public SeedBuilder<T> HasMany<TItem>(
        Expression<Func<T, List<TItem>>> selector,
        Action<SeedBuilder<TItem>> configure)
        where TItem : class
    {
        var nested = new SeedBuilder<TItem>();
        configure(nested);
        RuleFor(selector).UseFactory(() => nested.Build().ToList());
        return this;
    }

    /// <summary>
    /// Configures the builder to produce one entity for each element in <paramref name="items"/>,
    /// applying <paramref name="configure"/> to set item-specific rules on each entity.
    /// Calling <see cref="ForEach{TItem}"/> multiple times creates a cartesian product —
    /// one entity is generated for every unique combination across all calls.
    /// When <see cref="ForEach{TItem}"/> is used, any <see cref="Count(int)"/> setting is ignored.
    /// </summary>
    /// <typeparam name="TItem">The type of element driving the iteration.</typeparam>
    /// <param name="items">The source collection. An empty collection produces no entities.</param>
    /// <param name="configure">
    /// A delegate that receives the child builder and the current item.
    /// Rules registered here override base rules for the same property.
    /// </param>
    public SeedBuilder<T> ForEach<TItem>(
        IEnumerable<TItem> items,
        Action<SeedBuilder<T>, TItem> configure)
    {
        _forEachUsed = true;
        _forEachDimensions ??= new();
        var dimension = items
            .Select<TItem, Action<SeedBuilder<T>>>(item => b => configure(b, item))
            .ToList();
        if (dimension.Count > 0)
            _forEachDimensions.Add(dimension);
        return this;
    }

    /// <summary>
    /// Configures a collection property typed as <see cref="IEnumerable{TItem}"/> by building one item
    /// for each element in <paramref name="sources"/>, applying <paramref name="configure"/> per item.
    /// Equivalent to calling <c>HasMany(selector, b => b.ForEach(sources, configure))</c>.
    /// </summary>
    /// <typeparam name="TItem">The element type of the collection. Must be a reference type.</typeparam>
    /// <typeparam name="TSource">The type of element driving the iteration.</typeparam>
    /// <param name="selector">A member expression identifying the direct collection property to populate.</param>
    /// <param name="sources">The source collection that determines how many items are produced.</param>
    /// <param name="configure">A delegate that configures the nested builder for each source element.</param>
    public SeedBuilder<T> HasMany<TItem, TSource>(
        Expression<Func<T, IEnumerable<TItem>>> selector,
        IEnumerable<TSource> sources,
        Action<SeedBuilder<TItem>, TSource> configure)
        where TItem : class
    {
        var nested = new SeedBuilder<TItem>();
        nested.ForEach(sources, configure);
        RuleFor(selector).UseFactory(() => nested.Build());
        return this;
    }

    /// <summary>
    /// Configures a collection property typed as <see cref="ICollection{TItem}"/> by building one item
    /// for each element in <paramref name="sources"/>, applying <paramref name="configure"/> per item.
    /// Equivalent to calling <c>HasMany(selector, b => b.ForEach(sources, configure))</c>.
    /// </summary>
    /// <typeparam name="TItem">The element type of the collection. Must be a reference type.</typeparam>
    /// <typeparam name="TSource">The type of element driving the iteration.</typeparam>
    /// <param name="selector">A member expression identifying the direct collection property to populate.</param>
    /// <param name="sources">The source collection that determines how many items are produced.</param>
    /// <param name="configure">A delegate that configures the nested builder for each source element.</param>
    public SeedBuilder<T> HasMany<TItem, TSource>(
        Expression<Func<T, ICollection<TItem>>> selector,
        IEnumerable<TSource> sources,
        Action<SeedBuilder<TItem>, TSource> configure)
        where TItem : class
    {
        var nested = new SeedBuilder<TItem>();
        nested.ForEach(sources, configure);
        RuleFor(selector).UseFactory(() => nested.Build().ToList());
        return this;
    }

    /// <summary>
    /// Configures a collection property typed as <see cref="List{TItem}"/> by building one item
    /// for each element in <paramref name="sources"/>, applying <paramref name="configure"/> per item.
    /// Equivalent to calling <c>HasMany(selector, b => b.ForEach(sources, configure))</c>.
    /// </summary>
    /// <typeparam name="TItem">The element type of the collection. Must be a reference type.</typeparam>
    /// <typeparam name="TSource">The type of element driving the iteration.</typeparam>
    /// <param name="selector">A member expression identifying the direct collection property to populate.</param>
    /// <param name="sources">The source collection that determines how many items are produced.</param>
    /// <param name="configure">A delegate that configures the nested builder for each source element.</param>
    public SeedBuilder<T> HasMany<TItem, TSource>(
        Expression<Func<T, List<TItem>>> selector,
        IEnumerable<TSource> sources,
        Action<SeedBuilder<TItem>, TSource> configure)
        where TItem : class
    {
        var nested = new SeedBuilder<TItem>();
        nested.ForEach(sources, configure);
        RuleFor(selector).UseFactory(() => nested.Build().ToList());
        return this;
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
    /// When <see cref="ForEach{TItem}"/> has been called, one entity is produced per item combination
    /// (cartesian product across all <see cref="ForEach{TItem}"/> calls) and the <c>Count</c> setting is ignored.
    /// </summary>
    /// <returns>
    /// A collection of seeded <typeparamref name="T"/> instances.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when a registered rule has no value or factory configured.
    /// See <see cref="SeedRule{T,TProperty}.Apply"/>.
    /// </exception>
    public IEnumerable<T> Build()
    {
        if (_forEachUsed)
            return _forEachDimensions is { Count: > 0 } ? BuildForEach() : Array.Empty<T>();

        var sortedRules = TopologicalSort(_rules);

        if (_baseEntities != null)
        {
            var pool = _baseEntities.ToList();
            if (pool.Count == 0) return Array.Empty<T>();

            var count = _countExplicitlySet
                ? Random.Shared.Next(_countMin, _countMax + 1)
                : pool.Count;

            var entities = new List<T>(count);
            for (int i = 0; i < count; i++)
                entities.Add(CreateInstance(sortedRules, _nextIndex++, pool[i % pool.Count]));
            return entities;
        }

        {
            var count = Random.Shared.Next(_countMin, _countMax + 1);
            var entities = new List<T>(count);
            for (int i = 0; i < count; i++)
                entities.Add(CreateInstance(sortedRules, _nextIndex++));
            return entities;
        }
    }

    private IEnumerable<T> BuildForEach()
    {
        // Compute the cartesian product of all ForEach dimensions.
        // Each dimension is a list of item-specific configurators; one configurator is chosen per dimension.
        IEnumerable<IReadOnlyList<Action<SeedBuilder<T>>>> combos =
            new[] { (IReadOnlyList<Action<SeedBuilder<T>>>) Array.Empty<Action<SeedBuilder<T>>>() };

        foreach (var dimension in _forEachDimensions!)
        {
            combos = combos.SelectMany(
                combo => dimension,
                (combo, action) => (IReadOnlyList<Action<SeedBuilder<T>>>) new List<Action<SeedBuilder<T>>>(combo) { action });
        }

        var pool = _baseEntities?.ToList();
        var entities = new List<T>();
        int comboIndex = 0;
        foreach (var combo in combos)
        {
            // Build a child that inherits the base rules, then apply per-item overrides.
            // If both the base and an override target the same property, both rules run in order
            // so the override's value wins (last writer wins via the property setter).
            var child = new SeedBuilder<T> { _factory = _factory };
            foreach (var rule in _rules)
                child._rules.Add(rule);
            foreach (var action in combo)
                action(child);

            var sortedRules = TopologicalSort(child._rules);
            T? baseEntity = pool is { Count: > 0 } ? pool[comboIndex % pool.Count] : null;
            entities.Add(child.CreateInstance(sortedRules, _nextIndex++, baseEntity));
            comboIndex++;
        }
        return entities;
    }

    private T CreateInstance(List<ISeedRule<T>> sortedRules, int index, T? baseEntity = null)
    {
        var entity = baseEntity ?? (_factory != null ? _factory() : Activator.CreateInstance<T>()!);
        foreach (var rule in sortedRules)
            rule.Apply(entity, index);
        return entity;
    }

    /// <summary>
    /// Sorts <paramref name="rules"/> so that every rule runs after all rules it depends on,
    /// using Kahn's algorithm (BFS-based topological sort).
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a circular dependency is detected.</exception>
    private static List<ISeedRule<T>> TopologicalSort(List<ISeedRule<T>> rules)
    {
        int n = rules.Count;

        // Map property name -> indices of rules that target that property
        var nameToIndices = new Dictionary<string, List<int>>(n);
        for (int i = 0; i < n; i++)
        {
            var name = rules[i].PropertyName;
            if (!nameToIndices.TryGetValue(name, out var list))
                nameToIndices[name] = list = new List<int>();
            list.Add(i);
        }

        // Build adjacency: depIdx -> list of rule indices that directly depend on it
        var inDegree = new int[n];
        var dependents = new List<int>[n];
        for (int i = 0; i < n; i++) dependents[i] = new List<int>();

        for (int i = 0; i < n; i++)
        {
            foreach (var depName in rules[i].Dependencies)
            {
                if (!nameToIndices.TryGetValue(depName, out var depIndices))
                    continue; // dependency not covered by any rule = no ordering constraint

                foreach (var depIdx in depIndices)
                {
                    dependents[depIdx].Add(i);
                    inDegree[i]++;
                }
            }
        }

        // Kahn's BFS: seed queue with zero-in-degree rules in registration order
        var queue = new Queue<int>();
        for (int i = 0; i < n; i++)
            if (inDegree[i] == 0) queue.Enqueue(i);

        var result = new List<ISeedRule<T>>(n);
        while (queue.Count > 0)
        {
            var idx = queue.Dequeue();
            result.Add(rules[idx]);
            foreach (var dep in dependents[idx])
                if (--inDegree[dep] == 0)
                    queue.Enqueue(dep);
        }

        if (result.Count != n)
            throw new InvalidOperationException(
                "A circular dependency was detected among seed rules. Check your DependsOn declarations.");

        return result;
    }
}
