using System.Security.Claims;
using LeanCode.DomainModels.Ids;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace LeanCode.UserIdExtractors.Tests;

[TypedId(TypedIdFormat.RawInt)]
public readonly partial record struct TestIntId;

[TypedId(TypedIdFormat.RawLong)]
public readonly partial record struct TestLongId;

[TypedId(TypedIdFormat.RawGuid)]
public readonly partial record struct TestGuidId;

[TypedId(TypedIdFormat.PrefixedUlid, CustomPrefix = "tpl")]
public readonly partial record struct TestPrefixedUlidId;

[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "tpg")]
public readonly partial record struct TestPrefixedGuidId;

public class UserIdExtractorsTests
{
    private const string UserIdClaim = "sub";

    [Fact]
    public void String_user_id_can_be_extracted_from_claims_principal()
    {
        var serviceProvider = AddUserIdExtractor<string>();

        var userId = Guid.NewGuid();
        var user = CreateUser(userId.ToString());

        var stringUserIdExtractor = serviceProvider.GetRequiredService<IUserIdExtractor>();
        var extractedUserId = stringUserIdExtractor.Extract(user);

        Assert.Equal(userId.ToString(), extractedUserId);
    }

    [Fact]
    public void Generic_string_user_id_can_be_extracted_from_claims_principal()
    {
        var serviceProvider = AddUserIdExtractor<string>();

        var userId = Guid.NewGuid();
        var user = CreateUser(userId.ToString());

        var genericStringUserIdExtractor = serviceProvider.GetRequiredService<IUserIdExtractor<string>>();
        var genericExtractedUserId = genericStringUserIdExtractor.Extract(user);

        Assert.Equal(userId.ToString(), genericExtractedUserId);
    }

    [Fact]
    public void Guid_user_id_can_be_extracted_from_claims_principal()
    {
        var serviceProvider = AddUserIdExtractor<Guid>();

        var userId = Guid.NewGuid();
        var user = CreateUser(userId.ToString());

        var guidUserIdExtractor = serviceProvider.GetRequiredService<IUserIdExtractor<Guid>>();
        var extractedUserId = guidUserIdExtractor.Extract(user);

        Assert.Equal(userId, extractedUserId);
    }

    [Fact]
    public void Raw_typed_guid_user_id_can_be_extracted_from_claims_principal()
    {
        var serviceProvider = AddUserIdExtractor<TestGuidId>();

        var rawUserId = TestGuidId.New();
        var user = CreateUser(rawUserId.ToString());

        var rawTypedUserIdExtractor = serviceProvider.GetRequiredService<IUserIdExtractor<TestGuidId>>();
        var extractedUserId = rawTypedUserIdExtractor.Extract(user);

        Assert.Equal(rawUserId, extractedUserId);
    }

    [Fact]
    public void Prefixed_typed_guid_user_id_can_be_extracted_from_claims_principal()
    {
        var serviceProvider = AddUserIdExtractor<TestPrefixedGuidId>();

        var prefixedUserId = TestPrefixedGuidId.New();
        var user = CreateUser(prefixedUserId);

        var prefixedTypedUserIdExtractor = serviceProvider.GetRequiredService<IUserIdExtractor<TestPrefixedGuidId>>();
        var extractedUserId = prefixedTypedUserIdExtractor.Extract(user);

        Assert.Equal(prefixedUserId, extractedUserId);
    }

    [Fact]
    public void Non_generic_IUserIdExtractor_is_registered_for_other_user_id_types()
    {
        var serviceProvider = AddUserIdExtractor<TestPrefixedGuidId>();

        Assert.NotNull(serviceProvider.GetService<IUserIdExtractor>());
    }

    [Fact]
    public void AddUserIdExtractor_throws_when_provided_user_id_is_not_supported()
    {
        Assert.Throws<InvalidOperationException>(() => AddUserIdExtractor<bool>());
    }

    private static ServiceProvider AddUserIdExtractor<TUserId>()
        where TUserId : notnull, IEquatable<TUserId>
    {
        var services = new ServiceCollection();

        services.AddUserIdExtractor<TUserId>(UserIdClaim);

        return services.BuildServiceProvider();
    }

    private static ClaimsPrincipal CreateUser(string userId)
    {
        var claim = new Claim(UserIdClaim, userId);

        return new ClaimsPrincipal(new ClaimsIdentity([ claim ]));
    }
}
