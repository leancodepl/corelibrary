using LeanCode.UserIdExtractors.Extractors;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.UserIdExtractors.Tests;

public class UserIdExtractorsRegistrationTests
{
    [Fact]
    public void String_IUserIdExtractor_is_correctly_registered()
    {
        var serviceProvider = BuildServiceProvider(services => services.AddStringUserIdExtractor());
        var userIdExtractor = serviceProvider.GetService<IUserIdExtractor<string>>();

        Assert.NotNull(userIdExtractor);
        Assert.IsType<GenericStringUserIdExtractor>(userIdExtractor);
    }

    [Fact]
    public void Guid_IUserIdExtractor_is_correctly_registered()
    {
        var serviceProvider = BuildServiceProvider(services => services.AddGuidUserIdExtractor());
        var userIdExtractor = serviceProvider.GetService<IUserIdExtractor<Guid>>();

        Assert.NotNull(userIdExtractor);
        Assert.IsType<GuidUserIdExtractor>(userIdExtractor);
    }

    [Fact]
    public void Raw_typed_IUserIdExtractor_is_correctly_registered()
    {
        var serviceProvider = BuildServiceProvider(services => services.AddRawTypedUserIdExtractor<Guid, TestGuidId>());
        var userIdExtractor = serviceProvider.GetService<IUserIdExtractor<TestGuidId>>();

        Assert.NotNull(userIdExtractor);
        Assert.IsType<RawTypedUserIdExtractor<Guid, TestGuidId>>(userIdExtractor);
    }

    [Fact]
    public void Prefixed_typed_IUserIdExtractor_is_correctly_registered()
    {
        var serviceProvider = BuildServiceProvider(services =>
            services.AddPrefixedUserIdExtractor<TestPrefixedGuidId>()
        );
        var userIdExtractor = serviceProvider.GetService<IUserIdExtractor<TestPrefixedGuidId>>();

        Assert.NotNull(userIdExtractor);
        Assert.IsType<PrefixedTypedUserIdExtractor<TestPrefixedGuidId>>(userIdExtractor);
    }

    [Fact]
    public void Non_generic_IUserIdExtractor_is_registered_for_other_user_id_types()
    {
        var serviceProvider = BuildServiceProvider(services =>
            services.AddPrefixedUserIdExtractor<TestPrefixedGuidId>()
        );
        var userIdExtractor = serviceProvider.GetService<IUserIdExtractor>();

        Assert.NotNull(userIdExtractor);
        Assert.IsType<StringUserIdExtractor>(userIdExtractor);
    }

    private static ServiceProvider BuildServiceProvider(Action<ServiceCollection> registrationAction)
    {
        var services = new ServiceCollection();

        registrationAction(services);

        return services.BuildServiceProvider();
    }
}
