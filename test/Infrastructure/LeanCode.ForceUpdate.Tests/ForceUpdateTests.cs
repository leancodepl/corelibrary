using LeanCode.ForceUpdate.Contracts;
using FluentAssertions;
using Xunit;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using LeanCode.CQRS.AspNetCore;
using Microsoft.AspNetCore.Http;
using LeanCode.CQRS.Execution;

namespace LeanCode.ForceUpdate.Tests;

public class ForceUpdateTests
{
    private const string AndroidMinimumRequiredVersion = "2.0";
    private const string AndroidCurrentlySupportedVersion = "2.3";
    private const string IOSMinimumRequiredVersion = "1.0";
    private const string IOSCurrentlySupportedVersion = "1.3";

    private readonly ServiceProvider serviceProvider;

    public ForceUpdateTests()
    {
        var services = new ServiceCollection();
        services
            .AddCQRS(new(Array.Empty<Assembly>()), new(Array.Empty<Assembly>()))
            .AddForceUpdate(
                new AndroidVersionsConfiguration(
                    new Version(AndroidMinimumRequiredVersion),
                    new Version(AndroidCurrentlySupportedVersion)
                ),
                new IOSVersionsConfiguration(
                    new Version(IOSMinimumRequiredVersion),
                    new Version(IOSCurrentlySupportedVersion)
                )
            );

        this.serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Version_smaller_then_minimum_required_is_not_supported()
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<VersionSupport, VersionSupportDTO?>>();
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.IOS, Version = "0.9", }
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
    public async Task Update_is_suggested_for_version_between_minium_and_current()
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<VersionSupport, VersionSupportDTO?>>();
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.Android, Version = "2.2", }
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
        var handler = serviceProvider.GetRequiredService<IQueryHandler<VersionSupport, VersionSupportDTO?>>();
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.IOS, Version = "1.4", }
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
    public async Task Version_support_returns_null_for_invalid_version()
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<VersionSupport, VersionSupportDTO?>>();
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.IOS, Version = "1.x", }
        );

        result.Should().BeNull();
    }
}
