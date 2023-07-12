namespace LeanCode.CQRS.Annotations;

/// <summary>
/// Register command/query/operation under additional HTTP path. This could be useful for reorganizing contracts namespaces without introducing a breaking change.
/// This attribute can be used multiple times.
/// </summary>
/// <example>
/// <code>
/// namespace LncdApp.Contracts;
/// {
///     [PathAlias("LncdApp.Contacts.Commands")]
///     public class MyCommand : ICommand { }
/// }
/// </code>
/// <br/>
/// MyCommand class will be registered as 'cqrs-base/command/LncdApp.Contracts.MyCommand' and 'cqrs-base/command/LncdApp.Contracts.Commands.MyCommand'
/// </example>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class PathAliasAttribute : Attribute
{
    public string Path { get; }

    public PathAliasAttribute(string path)
    {
        Path = path;
    }
}
