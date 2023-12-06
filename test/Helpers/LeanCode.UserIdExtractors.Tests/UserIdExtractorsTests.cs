using System.Reflection;
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

    private readonly IServiceProvider serviceProvider;

    public UserIdExtractorsTests()
    {
        var services = new ServiceCollection();

        services.AddUserIdExtractors([ Assembly.GetExecutingAssembly() ], UserIdClaim);

        serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public void String_user_id_can_be_extracted_from_claims_principal()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId.ToString());

        var stringUserIdExtractor = serviceProvider.GetRequiredService<IUserIdExtractor>();
        var extractedUserId = stringUserIdExtractor.Extract(user);

        Assert.Equal(userId.ToString(), extractedUserId);
    }

    [Fact]
    public void Generic_string_user_id_can_be_extracted_from_claims_principal()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId.ToString());

        var genericStringUserIdExtractor = serviceProvider.GetRequiredService<IUserIdExtractor<string>>();
        var genericExtractedUserId = genericStringUserIdExtractor.ExtractId(user);

        Assert.Equal(userId.ToString(), genericExtractedUserId);
    }

    [Fact]
    public void Guid_user_id_can_be_extracted_from_claims_principal()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId.ToString());

        var guidUserIdExtractor = serviceProvider.GetRequiredService<IUserIdExtractor<Guid>>();
        var extractedUserId = guidUserIdExtractor.ExtractId(user);

        Assert.Equal(userId, extractedUserId);
    }

    [Fact]
    public void Raw_typed_guid_user_id_can_be_extracted_from_claims_principal()
    {
        var rawUserId = TestGuidId.New();
        var user = CreateUser(rawUserId.ToString());

        var rawTypedUserIdExtractor = serviceProvider.GetRequiredService<IUserIdExtractor<TestGuidId>>();
        var extractedUserId = rawTypedUserIdExtractor.ExtractId(user);

        Assert.Equal(rawUserId, extractedUserId);
    }

    [Fact]
    public void Prefixed_typed_guid_user_id_can_be_extracted_from_claims_principal()
    {
        var prefixedUserId = TestPrefixedGuidId.New();
        var user = CreateUser(prefixedUserId);

        var prefixedTypedUserIdExtractor = serviceProvider.GetRequiredService<IUserIdExtractor<TestPrefixedGuidId>>();
        var extractedUserId = prefixedTypedUserIdExtractor.ExtractId(user);

        Assert.Equal(prefixedUserId, extractedUserId);
    }

    private static ClaimsPrincipal CreateUser(string userId)
    {
        var claim = new Claim(UserIdClaim, userId);

        return new ClaimsPrincipal(new ClaimsIdentity([ claim ]));
    }
}
