using LeanCode.UserIdExtractors.Extractors;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.UserIdExtractors.Tests;

public class UserIdExtractorsRegistrationTests
{
    private const string UserIdClaim = "sub";

    [Fact]
    public void String_IUserIdExtractor_is_correctly_registered()
    {
        var serviceProvider = BuildServiceProvider(services => services.AddStringUserIdExtractor(UserIdClaim));
        var userIdExtractor = serviceProvider.GetService<IUserIdExtractor<string>>();

        Assert.NotNull(userIdExtractor);
        Assert.IsType<GenericStringUserIdExtractor>(userIdExtractor);
    }

    [Fact]
    public void Guid_IUserIdExtractor_is_correctly_registered()
    {
        var serviceProvider = BuildServiceProvider(services => services.AddGuidUserIdExtractor(UserIdClaim));
        var userIdExtractor = serviceProvider.GetService<IUserIdExtractor<Guid>>();

        Assert.NotNull(userIdExtractor);
        Assert.IsType<GuidUserIdExtractor>(userIdExtractor);
    }

    [Fact]
    public void Raw_typed_IUserIdExtractor_is_correctly_registered()
    {
        var serviceProvider = BuildServiceProvider(
            services => services.AddRawTypedUserIdExtractor<int, TestIntId>(UserIdClaim)
        );
        var userIdExtractor = serviceProvider.GetService<IUserIdExtractor<TestIntId>>();

        Assert.NotNull(userIdExtractor);
        Assert.IsType<RawTypedUserIdExtractor<int, TestIntId>>(userIdExtractor);
    }

    [Fact]
    public void Prefixed_typed_IUserIdExtractor_is_correctly_registered()
    {
        var serviceProvider = BuildServiceProvider(
            services => services.AddPrefixedUserIdExtractor<TestPrefixedGuidId>(UserIdClaim)
        );
        var userIdExtractor = serviceProvider.GetService<IUserIdExtractor<TestPrefixedGuidId>>();

        Assert.NotNull(userIdExtractor);
        Assert.IsType<PrefixedTypedUserIdExtractor<TestPrefixedGuidId>>(userIdExtractor);
    }

    [Fact]
    public void Non_generic_IUserIdExtractor_is_registered_for_other_user_id_types()
    {
        var serviceProvider = BuildServiceProvider(
            services => services.AddPrefixedUserIdExtractor<TestPrefixedGuidId>(UserIdClaim)
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
