using System.Reflection;
using FluentAssertions;
using LeanCode.CQRS.AspNetCore;
using LeanCode.CQRS.Execution;
using LeanCode.ForceUpdate.Contracts;
using LeanCode.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Semver;
using Xunit;

namespace LeanCode.ForceUpdate.Tests;

public class ForceUpdateTests
{
    private const string AndroidMinimumRequiredVersion = "2.0.0";
    private const string AndroidCurrentlySupportedVersion = "2.3.0";
    private const string IOSMinimumRequiredVersion = "1.0.0";
    private const string IOSCurrentlySupportedVersion = "1.3.0";

    private readonly IServiceProvider serviceProvider;
    private readonly IQueryHandler<VersionSupport, VersionSupportDTO> handler;

    public ForceUpdateTests()
    {
        var services = new ServiceCollection();

        services
            .AddLogging(logging => logging.AddNullLeanCodeLogger())
            .AddCQRS(new(Array.Empty<Assembly>()), new(Array.Empty<Assembly>()))
            .AddForceUpdate(
                new AndroidVersionsConfiguration(
                    SemVersion.Parse(AndroidMinimumRequiredVersion, SemVersionStyles.Any),
                    SemVersion.Parse(AndroidCurrentlySupportedVersion, SemVersionStyles.Any)
                ),
                new IOSVersionsConfiguration(
                    SemVersion.Parse(IOSMinimumRequiredVersion, SemVersionStyles.Any),
                    SemVersion.Parse(IOSCurrentlySupportedVersion, SemVersionStyles.Any)
                )
            );

        serviceProvider = services.BuildServiceProvider();
        handler = serviceProvider.GetRequiredService<IQueryHandler<VersionSupport, VersionSupportDTO>>();
    }

    [Fact]
    public async Task Version_smaller_than_minimum_required_is_not_supported()
    {
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.IOS, Version = "0.9" }
        );

        result
            .Should()
            .BeEquivalentTo(
                new VersionSupportDTO
                {
                    CurrentlySupportedVersion = IOSCurrentlySupportedVersion,
                    MinimumRequiredVersion = IOSMinimumRequiredVersion,
                    Result = VersionSupportResultDTO.UpdateRequired,
                }
            );
    }

    [Fact]
    public async Task Update_is_suggested_for_version_between_minimum_and_current()
    {
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.Android, Version = "2.2" }
        );

        result
            .Should()
            .BeEquivalentTo(
                new VersionSupportDTO
                {
                    CurrentlySupportedVersion = AndroidCurrentlySupportedVersion,
                    MinimumRequiredVersion = AndroidMinimumRequiredVersion,
                    Result = VersionSupportResultDTO.UpdateSuggested,
                }
            );
    }

    [Fact]
    public async Task Semver_pre_releases_are_respected()
    {
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.Android, Version = "2.3.0-beta1" }
        );

        result
            .Should()
            .BeEquivalentTo(
                new VersionSupportDTO
                {
                    CurrentlySupportedVersion = AndroidCurrentlySupportedVersion,
                    MinimumRequiredVersion = AndroidMinimumRequiredVersion,
                    Result = VersionSupportResultDTO.UpdateSuggested,
                }
            );
    }

    [Fact]
    public async Task Version_above_currently_supported_is_up_to_date()
    {
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.IOS, Version = "1.4" }
        );

        result
            .Should()
            .BeEquivalentTo(
                new VersionSupportDTO
                {
                    CurrentlySupportedVersion = IOSCurrentlySupportedVersion,
                    MinimumRequiredVersion = IOSMinimumRequiredVersion,
                    Result = VersionSupportResultDTO.UpToDate,
                }
            );
    }

    [Fact]
    public async Task Returns_up_to_date_result_for_invalid_version()
    {
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.IOS, Version = "1.x" }
        );

        result.Should().BeEquivalentTo(new { Result = VersionSupportResultDTO.UpToDate });
    }

    [Fact]
    public async Task Returns_zero_version_if_platform_cannot_be_deduced()
    {
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = (PlatformDTO)100, Version = "1.0" }
        );

        result.Should().BeEquivalentTo(new { CurrentlySupportedVersion = "0.0.0", MinimumRequiredVersion = "0.0.0" });
    }

    [Fact]
    public async Task If_both_version_and_platform_are_invalid_returns_UpToDate_response_with_fake_version()
    {
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = (PlatformDTO)100, Version = "1.x" }
        );

        result
            .Should()
            .BeEquivalentTo(
                new VersionSupportDTO
                {
                    CurrentlySupportedVersion = "0.0.0",
                    MinimumRequiredVersion = "0.0.0",
                    Result = VersionSupportResultDTO.UpToDate,
                }
            );
    }
}
