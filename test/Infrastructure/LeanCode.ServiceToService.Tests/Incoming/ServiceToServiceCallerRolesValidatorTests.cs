using System.Collections.Frozen;
using FluentAssertions;
using FluentAssertions.Execution;
using LeanCode.CQRS.Security;
using LeanCode.ServiceToService.Incoming;
using Xunit;

namespace LeanCode.ServiceToService.Tests.Incoming;

public class ServiceToServiceCallerRolesValidatorTests
{
    [Fact]
    public void Succeeds_when_all_configured_roles_are_registered()
    {
        var validator = CreateValidator(["system_notifications_service", "system_admin_panel"]);
        var options = new ServiceToServiceAuthenticationOptions
        {
            CallerRoles = new Dictionary<string, FrozenSet<string>>
            {
                ["notifications-service"] = ["system_notifications_service"],
                ["admin-panel"] = ["system_admin_panel", "system_notifications_service"],
            }.ToFrozenDictionary(),
        };

        var result = validator.Validate(name: null, options);

        using var _ = new AssertionScope();
        result.Succeeded.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }

    [Fact]
    public void Fails_when_any_configured_role_is_not_registered()
    {
        var validator = CreateValidator(["system_notifications_service"]);
        var options = new ServiceToServiceAuthenticationOptions
        {
            CallerRoles = new Dictionary<string, FrozenSet<string>>
            {
                ["notifications-service"] = ["unknown-role", "system_notifications_service"],
            }.ToFrozenDictionary(),
        };

        var result = validator.Validate(name: null, options);

        using var _ = new AssertionScope();
        result.Failed.Should().BeTrue();
        result
            .Failures.Should()
            .ContainSingle()
            .Which.Should()
            .Be("Unknown role names configured in CallerRoles: unknown-role.");
    }

    [Fact]
    public void Skips_validation_when_disabled()
    {
        var validator = CreateValidator([]);
        var options = new ServiceToServiceAuthenticationOptions
        {
            ValidateCallerRolesAtStartup = false,
            CallerRoles = new Dictionary<string, FrozenSet<string>>
            {
                ["notifications-service"] = ["unknown-role"],
            }.ToFrozenDictionary(),
        };

        var result = validator.Validate(name: null, options);

        using var _ = new AssertionScope();
        result.Skipped.Should().BeTrue();
        result.Failures.Should().BeNullOrEmpty();
    }

    private static ServiceToServiceCallerRolesValidator CreateValidator(string[] registeredRoles)
    {
        var roleRegistry = new RoleRegistry([new TestRoleRegistration(registeredRoles.Select(r => new Role(r)))]);
        return new(roleRegistry);
    }

    private sealed class TestRoleRegistration : IRoleRegistration
    {
        public IEnumerable<Role> Roles { get; }

        public TestRoleRegistration(IEnumerable<Role> roles)
        {
            Roles = roles;
        }
    }
}
