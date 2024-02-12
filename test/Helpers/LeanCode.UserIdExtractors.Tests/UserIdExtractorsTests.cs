using System.Security.Claims;
using LeanCode.DomainModels.Ids;
using LeanCode.UserIdExtractors.Extractors;
using Xunit;

namespace LeanCode.UserIdExtractors.Tests;

[TypedId(TypedIdFormat.RawGuid)]
public readonly partial record struct TestGuidId;

[TypedId(TypedIdFormat.PrefixedGuid, CustomPrefix = "tpg")]
public readonly partial record struct TestPrefixedGuidId;

public class UserIdExtractorsTests
{
    private const string UserIdClaim = "sub";

    [Fact]
    public void String_user_id_can_be_extracted_from_claims_principal()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId.ToString());

        var stringUserIdExtractor = new StringUserIdExtractor(UserIdClaim);
        var extractedUserId = stringUserIdExtractor.Extract(user);

        Assert.Equal(userId.ToString(), extractedUserId);
    }

    [Fact]
    public void Guid_user_id_can_be_extracted_from_claims_principal()
    {
        var userId = Guid.NewGuid();
        var user = CreateUser(userId.ToString());

        var guidUserIdExtractor = new GuidUserIdExtractor(UserIdClaim);
        var extractedUserId = guidUserIdExtractor.Extract(user);

        Assert.Equal(userId, extractedUserId);
    }

    [Fact]
    public void Raw_typed_guid_user_id_can_be_extracted_from_claims_principal()
    {
        var rawUserId = TestGuidId.New();
        var user = CreateUser(rawUserId.ToString());

        var rawTypedUserIdExtractor = new RawTypedUserIdExtractor<Guid, TestGuidId>(UserIdClaim);
        var extractedUserId = rawTypedUserIdExtractor.Extract(user);

        Assert.Equal(rawUserId, extractedUserId);
    }

    [Fact]
    public void Prefixed_typed_guid_user_id_can_be_extracted_from_claims_principal()
    {
        var prefixedUserId = TestPrefixedGuidId.New();
        var user = CreateUser(prefixedUserId);

        var prefixedTypedUserIdExtractor = new PrefixedTypedUserIdExtractor<TestPrefixedGuidId>(UserIdClaim);
        var extractedUserId = prefixedTypedUserIdExtractor.Extract(user);

        Assert.Equal(prefixedUserId, extractedUserId);
    }

    private static ClaimsPrincipal CreateUser(string userId)
    {
        var claim = new Claim(UserIdClaim, userId);

        return new ClaimsPrincipal(new ClaimsIdentity([claim]));
    }
}
