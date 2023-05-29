using System.Reflection;
using MassTransit;
using MassTransit.Internals;
using MassTransit.Metadata;
using MassTransit.Util;

namespace LeanCode.DomainModels.MassTransitRelay;

public static class MassTransitRegistrationConfigurationExtensions
{
    // These methods are based on
    // https://github.com/MassTransit/MassTransit/blob/develop/src/MassTransit/Configuration/RegistrationExtensions.cs
    public static void AddConsumersWithDefaultConfiguration(
        this IRegistrationConfigurator configurator,
        IEnumerable<Assembly> assemblies,
        Type defaultDefinition
    )
    {
        var result = AssemblyTypeCache
            .FindTypes(assemblies, RegistrationMetadata.IsConsumerOrDefinition)
            .GetAwaiter()
            .GetResult();
        var types = result.FindTypes(TypeClassification.Closed | TypeClassification.Concrete).ToArray();
        configurator.AddConsumersWithDefaultConfiguration(types, defaultDefinition);
    }

    public static void AddConsumersWithDefaultConfiguration(
        this IRegistrationConfigurator configurator,
        Type[] types,
        Type defaultDefinition
    )
    {
        var outer = types.Where(MessageTypeCache.HasConsumerInterfaces);
        var inner = types.Where((Type x) => x.HasInterface(typeof(IConsumerDefinition<>)));
        var enumerable =
            from c in outer
            join d in inner on c equals d.GetClosingArgument(typeof(IConsumerDefinition<>)) into dc
            from d in dc.DefaultIfEmpty()
            select new { ConsumerType = c, DefinitionType = d };
        foreach (var item in enumerable)
        {
            configurator.AddConsumer(
                item.ConsumerType,
                item.DefinitionType ?? defaultDefinition.MakeGenericType(item.ConsumerType)
            );
        }
    }
}
