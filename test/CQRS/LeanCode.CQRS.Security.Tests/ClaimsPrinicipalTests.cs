using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LeanCode.CQRS.Security;
using LeanCode.CQRS.Security.Exceptions;
using NSubstitute;
using Xunit;

namespace LeanCode.CQRS.Default.Tests.Security;

public class ClaimsPrincipalTests
{
    private const string UserIdClaim = "sub";

    private static Guid userId = Guid.NewGuid();
    private static UserIdExtractor userIdExtractor = new();

    private static class Reg
    {
        public const string User = nameof(User);
        public const string Admin = nameof(Admin);
        public const string Contributor = nameof(Contributor);
    }

    private static class Permissions
    {
        public const string Read = nameof(Read);
        public const string List = nameof(List);
        public const string Create = nameof(Create);
        public const string Update = nameof(Update);
    }

    private readonly RoleRegistry registry;

    public ClaimsPrincipalTests()
    {
        registry = new RoleRegistry(new[] { new RoleRegistration() });
    }

    [Fact]
    public void User_with_single_role_is_authorized_when_has_permission()
    {
        var user = CreateUser(Reg.Admin);

        var hasPermission = user.HasPermission(registry, Permissions.Create);

        Assert.True(hasPermission);
    }

    [Fact]
    public void User_with_single_role_is_not_authorized_when_has_no_permission()
    {
        var user = CreateUser(Reg.User);

        var hasPermission = user.HasPermission(registry, Permissions.Create);

        Assert.False(hasPermission);
    }

    [Fact]
    public void User_with_single_role_is_authorized_when_has_at_least_one_of_required_permissions()
    {
        var user = CreateUser(Reg.User);

        var hasPermission = user.HasPermission(registry, Permissions.Create, Permissions.List, Permissions.Update);

        Assert.True(hasPermission);
    }

    [Fact]
    public void User_with_single_role_is_not_authorized_when_has_no_permissions()
    {
        var user = CreateUser(Reg.User);

        var hasPermission = user.HasPermission(registry, Permissions.Create, Permissions.Update);

        Assert.False(hasPermission);
    }

    [Fact]
    public void User_with_not_existing_role_is_not_authorized()
    {
        var user = CreateUser("NotExistingRole");

        var hasPermission = user.HasPermission(
            registry,
            Permissions.Create,
            Permissions.Update,
            Permissions.List,
            Permissions.Read
        );

        Assert.False(hasPermission);
    }

    [Fact]
    public void User_with_multiple_roles_is_authorized_when_has_permission()
    {
        var user = CreateUser(Reg.User, Reg.Contributor);

        var hasPermission = user.HasPermission(registry, Permissions.Update);

        Assert.True(hasPermission);
    }

    [Fact]
    public void User_with_multiple_roles_is_not_authorized_when_has_no_permission()
    {
        var user = CreateUser(Reg.User, Reg.Contributor);

        var hasPermission = user.HasPermission(registry, Permissions.Create);

        Assert.False(hasPermission);
    }

    [Fact]
    public void String_user_id_can_be_extracted_from_claims_principal()
    {
        var user = CreateUserWithId();

        var extractedUserId = ((IUserIdExtractor)userIdExtractor).Extract(user);

        Assert.Equal(userId.ToString(), extractedUserId);
    }

    [Fact]
    public void Guid_user_id_can_be_extracted_from_claims_principal()
    {
        var user = CreateUserWithId();

        var extractedUserId = userIdExtractor.Extract(user);

        Assert.Equal(userId, extractedUserId);
    }

    [Fact]
    public void User_id_extraction_fails_if_there_is_no_id_in_claims_principal()
    {
        var user = CreateUser();

        Assert.Throws<ArgumentNullException>(() => userIdExtractor.Extract(user));
    }

    private static ClaimsPrincipal CreateUser(params string[] roles)
    {
        var claims = roles.Select(r => new Claim("http://schemas.microsoft.com/ws/2008/06/identity/claims/role", r));

        return new ClaimsPrincipal(new ClaimsIdentity(claims));
    }

    private static ClaimsPrincipal CreateUserWithId()
    {
        var claim = new Claim(UserIdClaim, userId.ToString());

        return new ClaimsPrincipal(new ClaimsIdentity([ claim ]));
    }

    private sealed class RoleRegistration : IRoleRegistration
    {
        public IEnumerable<Role> Roles { get; } =
            new Role[]
            {
                new Role(Reg.User, Permissions.List, Permissions.Read),
                new Role(Reg.Admin, Permissions.Create, Permissions.List, Permissions.Read, Permissions.Update),
                new Role(Reg.Contributor, Permissions.Update),
            };
    }
}
