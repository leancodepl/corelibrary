using System.Reflection;
using Serilog;
using Serilog.Core;

namespace LeanCode.Logging;

internal static class SerilogExtensions
{
    internal static LoggerConfiguration DestructureCommonObjects(
        this LoggerConfiguration config,
        IEnumerable<Assembly>? searchAssemblies
    )
    {
        if (searchAssemblies != null)
        {
            return SelectTypes<IDestructuringPolicy>(searchAssemblies)
                .Aggregate(config, (a, s) => a.Destructure.With(s));
        }
        else
        {
            return config;
        }
    }

    private static List<TType> SelectTypes<TType>(IEnumerable<Assembly> searchAssemblies)
    {
        return searchAssemblies
            .SelectMany(a => a.ExportedTypes)
            .Where(t => typeof(TType).IsAssignableFrom(t) && t.IsPublic && t.GetConstructor(Type.EmptyTypes) != null)
            .Select(Activator.CreateInstance)
            .Cast<TType>()
            .ToList()!;
    }
}
