namespace LeanCode.Logging;

public interface ILogger<out T> : Serilog.ILogger
{
    public new ILogger<TNew> ForContext<TNew>() =>
        this is NullLogger<T> ? NullLogger<TNew>.Instance : new ContextualLogger<TNew>(this);
}
