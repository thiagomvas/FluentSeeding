using System.Linq.Expressions;

namespace FluentSeeding;

public sealed class SeedRule<T, TProperty> : ISeedRule<T>
where T : class
{
    private readonly Expression<Func<T, TProperty>> _selector;
    private readonly Action<T, TProperty> _setter;
    private readonly SeedBuilder<T> _parent;

    private Func<TProperty>? _valueFactory
    {
        get;
        set
        {
            if (field == value) return;
            _indexedValueFactory = null;
            field = value;
        }
    }

    private Func<int, TProperty>? _indexedValueFactory
    {
        get;
        set
        {
            if (field == value) return;
            _valueFactory = null;
            field = value;
        }
    }

    internal SeedRule(Expression<Func<T, TProperty>> selector, SeedBuilder<T> parent)
    {
        _selector = selector;
        _parent = parent;
        _setter = selector.BuildSetter();
    }

    public SeedBuilder<T> UseValue(TProperty value)
    {
        _valueFactory = () => value;
        return _parent;
    }

    public SeedBuilder<T> UseFactory(Func<TProperty> value)
    {
        _valueFactory = value;
        return _parent;
    }
    
    public SeedBuilder<T> UseFactory(Func<int, TProperty> value)
    {
        _indexedValueFactory = value;
        return _parent;
    }

    public SeedBuilder<T> UseFrom(params TProperty[] values)
    {
        if (values == null || values.Length == 0)
            throw new ArgumentException("Values collection cannot be null or empty.", nameof(values));
        var random = new Random();
        _valueFactory = () => values[random.Next(values.Length)];
        return _parent;
    }
    public SeedBuilder<T> UseFrom(IEnumerable<TProperty> values)
    {
        if (values == null || !values.Any())
            throw new ArgumentException("Values collection cannot be null or empty.", nameof(values));
        var random = new Random();
        _valueFactory = () => values.ElementAt(random.Next(values.Count()));
        return _parent;
    }
    
    public void Apply(T instance, int index = 0)
    {
        if (_valueFactory is null && _indexedValueFactory is null)
            throw new InvalidOperationException($"No value or factory configured for '{_selector}.");
        
        if (_valueFactory != null)
        {
            _setter(instance, _valueFactory());
        }
        else if (_indexedValueFactory != null)
        {
            _setter(instance, _indexedValueFactory(index));
        }
    }
}
