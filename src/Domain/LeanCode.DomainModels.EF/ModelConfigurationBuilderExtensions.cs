using System.Reflection;
using LeanCode.DomainModels.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LeanCode.DomainModels.EF;

public static class ModelConfigurationBuilderExtensions
{
    private static readonly MethodInfo AreIntTypedIdMethod = GetAreTypedIdMethod(
        nameof(PropertiesConfigurationBuilderExtensions.AreIntTypedId),
        nullable: false
    );
    private static readonly MethodInfo AreLongTypedIdMethod = GetAreTypedIdMethod(
        nameof(PropertiesConfigurationBuilderExtensions.AreLongTypedId),
        nullable: false
    );
    private static readonly MethodInfo AreGuidTypedIdMethod = GetAreTypedIdMethod(
        nameof(PropertiesConfigurationBuilderExtensions.AreGuidTypedId),
        nullable: false
    );
    private static readonly MethodInfo AreStringTypedIdMethod = GetAreTypedIdMethod(
        nameof(PropertiesConfigurationBuilderExtensions.AreStringTypedId),
        nullable: false
    );
    private static readonly MethodInfo ArePrefixedTypedIdMethod = GetAreTypedIdMethod(
        nameof(PropertiesConfigurationBuilderExtensions.ArePrefixedTypedId),
        nullable: false
    );

    private static readonly MethodInfo AreNullableIntTypedIdMethod = GetAreTypedIdMethod(
        nameof(PropertiesConfigurationBuilderExtensions.AreIntTypedId),
        nullable: true
    );
    private static readonly MethodInfo AreNullableLongTypedIdMethod = GetAreTypedIdMethod(
        nameof(PropertiesConfigurationBuilderExtensions.AreLongTypedId),
        nullable: true
    );
    private static readonly MethodInfo AreNullableGuidTypedIdMethod = GetAreTypedIdMethod(
        nameof(PropertiesConfigurationBuilderExtensions.AreGuidTypedId),
        nullable: true
    );
    private static readonly MethodInfo AreNullableStringTypedIdMethod = GetAreTypedIdMethod(
        nameof(PropertiesConfigurationBuilderExtensions.AreStringTypedId),
        nullable: true
    );
    private static readonly MethodInfo AreNullablePrefixedTypedIdMethod = GetAreTypedIdMethod(
        nameof(PropertiesConfigurationBuilderExtensions.ArePrefixedTypedId),
        nullable: true
    );

    private static MethodInfo GetAreTypedIdMethod(string name, bool nullable)
    {
        return typeof(PropertiesConfigurationBuilderExtensions)
            .GetMethods()
            .First(m =>
            {
                if (m.Name != name)
                {
                    return false;
                }

                var parameters = m.GetParameters();
                if (parameters.Length != 1)
                {
                    return false;
                }

                var parameterType = parameters[0].ParameterType;
                if (
                    !parameterType.IsGenericType
                    || parameterType.GetGenericTypeDefinition() != typeof(PropertiesConfigurationBuilder<>)
                )
                {
                    return false;
                }

                var genericArgument = parameterType.GetGenericArguments()[0];
                var isNullable =
                    genericArgument.IsGenericType && genericArgument.GetGenericTypeDefinition() == typeof(Nullable<>);

                return isNullable == nullable;
            });
    }

    /// <summary>
    /// Registers conventions for all types <see cref="IPrefixedTypedId{TSelf}"/>, <see cref="IRawStringTypedId{TSelf}"/> and <see cref="IRawTypedId{TBacking,TSelf}"/> defined in the assemblies.
    /// </summary>
    public static ModelConfigurationBuilder ConfigureTypedIdsConventions(
        this ModelConfigurationBuilder configurationBuilder,
        params Assembly[] assemblies
    )
    {
        foreach (var assembly in assemblies)
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.IsAbstract || !type.IsValueType)
                {
                    continue;
                }

                foreach (var iface in type.GetInterfaces())
                {
                    if (iface.IsGenericType)
                    {
                        var genericDefinition = iface.GetGenericTypeDefinition();
                        if (genericDefinition == typeof(IPrefixedTypedId<>) && iface.GetGenericArguments()[0] == type)
                        {
                            Configure(
                                configurationBuilder,
                                type,
                                ArePrefixedTypedIdMethod,
                                AreNullablePrefixedTypedIdMethod
                            );
                        }
                        else if (genericDefinition == typeof(IRawTypedId<,>) && iface.GetGenericArguments()[1] == type)
                        {
                            var backingType = iface.GetGenericArguments()[0];
                            if (backingType == typeof(int))
                            {
                                Configure(configurationBuilder, type, AreIntTypedIdMethod, AreNullableIntTypedIdMethod);
                            }
                            else if (backingType == typeof(long))
                            {
                                Configure(
                                    configurationBuilder,
                                    type,
                                    AreLongTypedIdMethod,
                                    AreNullableLongTypedIdMethod
                                );
                            }
                            else if (backingType == typeof(Guid))
                            {
                                Configure(
                                    configurationBuilder,
                                    type,
                                    AreGuidTypedIdMethod,
                                    AreNullableGuidTypedIdMethod
                                );
                            }
                        }
                        else if (
                            genericDefinition == typeof(IRawStringTypedId<>)
                            && iface.GetGenericArguments()[0] == type
                        )
                        {
                            Configure(
                                configurationBuilder,
                                type,
                                AreStringTypedIdMethod,
                                AreNullableStringTypedIdMethod
                            );
                        }
                    }
                }
            }
        }

        return configurationBuilder;
    }

    private static void Configure(
        ModelConfigurationBuilder configurationBuilder,
        Type type,
        MethodInfo method,
        MethodInfo nullableMethod
    )
    {
        var propertiesMethod = typeof(ModelConfigurationBuilder).GetMethod(
            nameof(ModelConfigurationBuilder.Properties),
            BindingFlags.Public | BindingFlags.Instance,
            []
        )!;

        // Configure non-nullable
        var propertiesBuilder = propertiesMethod.MakeGenericMethod(type).Invoke(configurationBuilder, null);
        method.MakeGenericMethod(type).Invoke(null, [propertiesBuilder]);

        // Configure nullable
        var nullableType = typeof(Nullable<>).MakeGenericType(type);
        var nullablePropertiesBuilder = propertiesMethod
            .MakeGenericMethod(nullableType)
            .Invoke(configurationBuilder, null);
        nullableMethod.MakeGenericMethod(type).Invoke(null, [nullablePropertiesBuilder]);
    }
}
