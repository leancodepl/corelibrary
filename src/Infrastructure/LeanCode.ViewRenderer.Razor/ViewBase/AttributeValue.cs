using System;

namespace LeanCode.ViewRenderer.Razor.ViewBase;

public class AttributeValue
{
    public string Prefix { get; }
    public object Value { get; }
    public bool Literal { get; }

    public AttributeValue(string prefix, object value, bool literal)
    {
        Prefix = prefix;
        Value = value;
        Literal = literal;
    }

    public static AttributeValue FromTuple(Tuple<string, object, bool> value) =>
        new AttributeValue(value.Item1, value.Item2, value.Item3);

    public static AttributeValue FromTuple(Tuple<string, string, bool> value) =>
        new AttributeValue(value.Item1, value.Item2, value.Item3);

    public static implicit operator AttributeValue(Tuple<string, object, bool> value) =>
        FromTuple(value);
}
