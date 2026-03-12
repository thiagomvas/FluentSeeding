using System.Linq.Expressions;

namespace FluentSeeding;

public sealed class SeedRule<T, TProperty> : ISeedRule<T>
where T : class
{
    private readonly Expression<Func<T, TProperty>> _selector;
    private readonly Action<T, TProperty> _setter;
    private Func<TProperty>? _valueFactory;

    internal SeedRule(Expression<Func<T, TProperty>> selector)
    {
        _selector = selector;
        _setter = selector.BuildSetter();
    }

    public SeedRule<T, TProperty> UseValue(TProperty value)
    {
        _valueFactory = () => value;
        return this;
    }

    public SeedRule<T, TProperty> UseFactory(Func<TProperty> value)
    {
        _valueFactory = value;
        return this;
        
    }

    public SeedRule<T, TProperty> UseFrom(params TProperty[] values)
    {
        var random = new Random();
        _valueFactory = () => values[random.Next(values.Length)];
        return this;
    }

    public void Apply(T instance)
    {
        if (_valueFactory is null)
            throw new InvalidOperationException($"No value or factory configured for '{_selector}.");
        
        _setter(instance, _valueFactory());
    }
}
