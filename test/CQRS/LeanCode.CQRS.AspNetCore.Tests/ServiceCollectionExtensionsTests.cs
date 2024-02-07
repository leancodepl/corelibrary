using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.CQRS.AspNetCore.Tests;

internal interface IGenericService<T> { }

internal sealed class Type1 { }

internal sealed class Type2 { }

internal sealed class Type3 { }

internal sealed class Type1Type2Service : IGenericService<Type1>, IGenericService<Type2> { }

internal sealed class Type3Service : IGenericService<Type3> { }

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void Registers_implementations_of_generic_type()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.RegisterGenericTypes(
            TypesCatalog.Of<Type1>(),
            typeof(IGenericService<>),
            ServiceLifetime.Transient
        );
        using var serviceProvider = serviceCollection.BuildServiceProvider();

        using var scope = serviceProvider.CreateScope();

        var service1 = scope.ServiceProvider.GetService<IGenericService<Type1>>();
        var service2 = scope.ServiceProvider.GetService<IGenericService<Type2>>();
        var service3 = scope.ServiceProvider.GetService<IGenericService<Type3>>();

        Assert.IsType<Type1Type2Service>(service1);
        Assert.IsType<Type1Type2Service>(service2);
        Assert.IsType<Type3Service>(service3);
    }
}
