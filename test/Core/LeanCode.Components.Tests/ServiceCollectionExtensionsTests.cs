using LeanCode.Components;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.Tests;

public class ServiceCollectionExtensionsTests
{
    private const string Property1 = "test";
    private const int Property2 = 10;

    [Fact]
    public void Implemented_interfaces_are_registered_correctly()
    {
        var services = new ServiceCollection();
        services.TryRegisterWithImplementedInterfaces<Implementation<object>>();
        var serviceProvider = services.BuildServiceProvider();

        Assert.NotNull(serviceProvider.GetService<Service1<object>>());
        Assert.NotNull(serviceProvider.GetService<Service2>());
        Assert.NotNull(serviceProvider.GetService<Implementation<object>>());
    }

    [Fact]
    public void Implemented_interfaces_are_registered_correctly_with_factory_method()
    {
        var services = new ServiceCollection();

        services.TryRegisterWithImplementedInterfaces<Implementation<string>>(
            _ => new Implementation<string> { Property1 = Property1, Property2 = Property2, }
        );
        var serviceProvider = services.BuildServiceProvider();

        var service1 = serviceProvider.GetService<Service1<string>>();
        var service2 = serviceProvider.GetService<Service2>();
        var implementation = serviceProvider.GetService<Implementation<string>>();

        Assert.NotNull(service1);
        Assert.NotNull(service2);
        Assert.NotNull(implementation);

        Assert.Equal(Property1, service1.Property1);
        Assert.Equal(Property2, service2.Property2);
        Assert.Equal(Property1, implementation.Property1);
        Assert.Equal(Property2, implementation.Property2);
    }

    private sealed class Implementation<T> : Service1<T>, Service2
        where T : class
    {
        public T Property1 { get; set; }
        public int Property2 { get; set; }
    }

    private interface Service1<T>
        where T : class
    {
        public T Property1 { get; set; }
    }

    private interface Service2
    {
        public int Property2 { get; set; }
    }
}
