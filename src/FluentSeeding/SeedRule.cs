using System.Linq.Expressions;
using System.Reflection;

namespace FluentSeeding;

/// <summary>
/// Encapsulates how a single property of <typeparamref name="T"/> is populated during seeding.
/// Obtained via <see cref="SeedBuilder{T}.RuleFor{TProperty}"/>; chain one of
/// <see cref="UseValue"/>, <see cref="UseFactory(Func{TProperty})"/>, or <see cref="UseFrom(TProperty[])"/>
/// to configure the value source, which returns the parent <see cref="SeedBuilder{T}"/>.
/// </summary>
/// <typeparam name="T">The entity type that owns the property.</typeparam>
/// <typeparam name="TProperty">The type of the property being seeded.</typeparam>
public sealed class SeedRule<T, TProperty> : ISeedRule<T>
where T : class
{
    private readonly Expression<Func<T, TProperty>> _selector;
    private readonly Action<T, TProperty> _setter;
    public readonly SeedBuilder<T> Parent;
    private readonly HashSet<string> _dependencies = new();
    private Func<T, bool>? _condition;

    private HashSet<TProperty>? _seenValues;
    private bool _unique = false;
    private double? _nullChance;
    private Func<TProperty, TProperty>? _mutator;
    private Func<T, int, TProperty>? _factory;

    /// <inheritdoc />
    public string PropertyName { get; }

    /// <inheritdoc />
    public IReadOnlyCollection<string> Dependencies => _dependencies;

    internal SeedRule(Expression<Func<T, TProperty>> selector, SeedBuilder<T> parent)
    {
        _selector = selector;
        Parent = parent;
        _setter = selector.BuildSetter();
        PropertyName = ((MemberExpression)selector.Body).Member.Name;
    }

    #region  Modifiers

    /// <summary>
    /// Gives this rule a <paramref name="chance"/> (0.0–1.0) of producing <see langword="null"/> (or
    /// <see langword="default"/>) instead of invoking the configured value source.
    /// For example, <c>0.3</c> means roughly 30 % of seeded entities will have this property set to
    /// <see langword="null"/>. Intended for nullable properties.
    /// </summary>
    /// <param name="chance">Probability of a null result, in the range [0.0, 1.0].</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="chance"/> is outside [0.0, 1.0].</exception>
    public SeedRule<T, TProperty> Sometimes(double chance = 0.5)
    {
        if (chance < 0.0 || chance > 1.0)
            throw new ArgumentOutOfRangeException(nameof(chance), "Chance must be between 0.0 and 1.0.");
        _nullChance = chance;
        return this;
    }

    /// <summary>
    /// Applies <paramref name="mutator"/> to the generated value before it is assigned to the entity.
    /// The mutator receives the value produced by the configured value source and returns the final value.
    /// Runs after uniqueness is checked, so the mutated value is what gets assigned and not the raw one.
    /// </summary>
    /// <param name="mutator">A transform function from the generated value to the final value.</param>
    public SeedRule<T, TProperty> Mutate(Func<TProperty, TProperty> mutator)
    {
        _mutator = mutator;
        return this;
    }

    /// <summary>
    /// Marks this rule as requiring unique values across all seeded entities. If the configured value source produces duplicates, an exception will be thrown at runtime.
    /// </summary>
    public SeedRule<T, TProperty> Unique()
    {
        _seenValues ??= new HashSet<TProperty>();
        _unique = true;
        return this;
    }

    /// <summary>
    /// Applies this rule only when <paramref name="condition"/> returns <see langword="true"/> for the entity
    /// being seeded. Because rules run after their dependencies, the entity already has any depended-on
    /// properties set when the condition is evaluated.
    /// </summary>
    /// <param name="condition">A predicate that receives the partially-built entity instance.</param>
    public SeedRule<T, TProperty> When(Func<T, bool> condition)
    {
        _condition = condition;
        return this;
    }

    /// <summary>
    /// Declares that this rule must run after the rule that targets <paramref name="dependency"/>.
    /// <see cref="SeedBuilder{T}.Build"/> uses Kahn's topological sort to enforce this ordering.
    /// Can be chained to declare multiple dependencies.
    /// </summary>
    /// <typeparam name="TDep">The type of the dependency property.</typeparam>
    /// <param name="dependency">A direct member expression identifying the property to run first (e.g. <c>x => x.IsAdmin</c>).</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="dependency"/> is not a direct member access.</exception>
    public SeedRule<T, TProperty> DependsOn<TDep>(Expression<Func<T, TDep>> dependency)
    {
        if (dependency.Body is not MemberExpression { Expression: ParameterExpression } memberExpr)
            throw new ArgumentException(
                $"Dependency selector must target a direct property of {typeof(T).Name}. Nested property access is not supported.",
                nameof(dependency));

        _dependencies.Add(memberExpr.Member.Name);
        return this;
    }

    #endregion
    
    #region Terminals
    /// <summary>
    /// Assigns the same constant <paramref name="value"/> to this property on every seeded entity.
    /// </summary>
    public SeedBuilder<T> UseValue(TProperty value)
    {
        _factory = (_, _) => value;
        return Parent;
    }

    /// <summary>
    /// Invokes <paramref name="value"/> once per entity to produce the property value.
    /// Use this when each entity should receive a freshly generated value (e.g. <c>Guid.NewGuid</c>).
    /// </summary>
    public SeedBuilder<T> UseFactory(Func<TProperty> value)
    {
        _factory = (_, _) => value();
        return Parent;
    }

    /// <summary>
    /// Invokes <paramref name="value"/> once per entity, passing the entity's zero-based index in the seed batch.
    /// Useful for generating sequential or index-derived values (e.g. <c>i => $"User {i}"</c>).
    /// </summary>
    public SeedBuilder<T> UseFactory(Func<int, TProperty> value)
    {
        _factory = (_, i) => value(i);
        return Parent;
    }

    /// <summary>
    /// Invokes <paramref name="expression"/> once per entity, passing the partially-built entity instance.
    /// Direct property accesses on the entity (e.g. <c>e.FirstName</c>) are automatically registered as
    /// dependencies, so those rules always run first. Use <see cref="DependsOn{TDep}"/> to declare any
    /// additional dependencies that cannot be inferred from the expression.
    /// </summary>
    public SeedBuilder<T> UseEntityFactory(Expression<Func<T, TProperty>> expression)
    {
        RegisterEntityDependencies(expression.Body, expression.Parameters[0]);
        var compiled = expression.Compile();
        _factory = (e, _) => compiled(e);
        return Parent;
    }

    /// <summary>
    /// Invokes <paramref name="expression"/> once per entity, passing the partially-built entity instance
    /// and the entity's zero-based index in the seed batch.
    /// Direct property accesses on the entity are automatically registered as dependencies.
    /// </summary>
    public SeedBuilder<T> UseEntityFactory(Expression<Func<T, int, TProperty>> expression)
    {
        RegisterEntityDependencies(expression.Body, expression.Parameters[0]);
        _factory = expression.Compile();
        return Parent;
    }

    /// <summary>
    /// Picks a random value from <paramref name="values"/> for each entity.
    /// </summary>
    /// <param name="values">The pool of values to pick from. Must contain at least one element.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is null or empty.</exception>
    public SeedBuilder<T> UseFrom(params TProperty[] values)
    {
        if (values == null || values.Length == 0)
            throw new ArgumentException("Values collection cannot be null or empty.", nameof(values));
        _factory = (_, _) => values[Random.Shared.Next(values.Length)];
        return Parent;
    }

    /// <summary>
    /// Picks a random value from <paramref name="values"/> for each entity.
    /// </summary>
    /// <param name="values">The pool of values to pick from. Must contain at least one element.</param>
    /// <exception cref="ArgumentException">Thrown when <paramref name="values"/> is null or empty.</exception>
    public SeedBuilder<T> UseFrom(IEnumerable<TProperty> values)
    {
        var vals = values as TProperty[] ?? values.ToArray();
        if (values == null || !vals.Any())
            throw new ArgumentException("Values collection cannot be null or empty.", nameof(values));
        _factory = (_, _) => vals[Random.Shared.Next(vals.Length)];
        return Parent;
    }

    /// <summary>
    /// Assigns <see langword="null"/> (or <see langword="default"/> for value types) to this property on every
    /// seeded entity. Intended for nullable properties (e.g. <c>string?</c>, <c>int?</c>).
    /// </summary>
    public SeedBuilder<T> UseNull()
    {
        _factory = (_, _) => default!;
        return Parent;
    }
    #endregion
    /// <inheritdoc />
    /// <exception cref="InvalidOperationException">
    /// Thrown when neither a value nor a factory has been configured for this rule.
    /// Ensure <see cref="UseValue"/>, <see cref="UseFactory(Func{TProperty})"/>, or <see cref="UseFrom(TProperty[])"/> was called.
    /// </exception>
    public void Apply(T instance, int index = 0)
    {
        if (_condition != null && !_condition(instance))
            return;

        var val = GenerateValue(instance, index);
        _setter(instance, val);
    }
    
    private TProperty InvokeFactory(T instance, int index) =>
        _factory != null
            ? _factory(instance, index)
            : throw new InvalidOperationException($"No value or factory configured for '{_selector}'.");

    private TProperty GenerateValue(T instance, int index)
    {
        if (_nullChance.HasValue && Random.Shared.NextDouble() < _nullChance.Value)
            return default!;

        var value = InvokeFactory(instance, index);

        if (_unique)
        {
            int attempts = 0;
            while (!_seenValues!.Add(value))
            {
                if (attempts++ > 100)
                    throw new InvalidOperationException($"Unable to generate a unique value for '{_selector}' after 100 attempts. Consider expanding the value pool, changing your value factory or removing the uniqueness requirement.");
                value = InvokeFactory(instance, index);
            }
        }

        return _mutator != null ? _mutator(value) : value;
    }

    private void RegisterEntityDependencies(Expression body, ParameterExpression entityParam)
    {
        var visitor = new DirectMemberVisitor(entityParam);
        visitor.Visit(body);
        foreach (var name in visitor.Members)
            _dependencies.Add(name);
    }

    private sealed class DirectMemberVisitor(ParameterExpression entityParam) : ExpressionVisitor
    {
        public readonly HashSet<string> Members = new();

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression == entityParam)
            {
                Members.Add(node.Member.Name);
                return node; 
            }
            return base.VisitMember(node);
        }
    }
}
