namespace LeanCode.Localization;

public class LocalizedResourceNotFoundException : Exception
{
    public LocalizedResourceNotFoundException(Exception inner)
        : base(inner.Message, inner) { }
}
