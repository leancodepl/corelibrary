using System.Collections;
using Microsoft.AspNetCore.Http.Features;

namespace LeanCode.CQRS.AspNetCore.Local;

internal class LocalFeatureCollection : IFeatureCollection
{
    private readonly IFeatureCollection inner;
    private readonly FeatureCollection overrides;

    public LocalFeatureCollection(IFeatureCollection inner)
    {
        this.inner = inner;
        this.overrides = new FeatureCollection(5);
    }

    public TFeature? Get<TFeature>()
    {
        return overrides.Get<TFeature>() ?? inner.Get<TFeature>();
    }

    public object? this[Type key]
    {
        get => overrides[key] ?? inner[key];
        set => overrides[key] = value;
    }
    public bool IsReadOnly => overrides.IsReadOnly;
    public int Revision => overrides.Revision;

    public void Set<TFeature>(TFeature? instance) => overrides.Set(instance);

    // TODO: support properly
    public IEnumerator<KeyValuePair<Type, object>> GetEnumerator() => inner.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)inner).GetEnumerator();
}
