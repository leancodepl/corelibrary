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
    private const string MinimumRequiredVersion = "1.0";
    private const string CurrentlySupportedVersion = "1.3";

    private readonly ServiceProvider serviceProvider;

    public ForceUpdateTests()
    {
        var services = new ServiceCollection();
        services.AddCQRS(new(Array.Empty<Assembly>()), new(Array.Empty<Assembly>())).AddForceUpdate();
        services.AddSingleton(
            new IOSVersionsConfiguration(new Version(MinimumRequiredVersion), new Version(CurrentlySupportedVersion))
        );
        services.AddSingleton(
            new AndroidVersionsConfiguration(
                new Version(MinimumRequiredVersion),
                new Version(CurrentlySupportedVersion)
            )
        );

        this.serviceProvider = services.BuildServiceProvider();
    }

    [Fact]
    public async Task Versions_correctly_returns_all_versions()
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<Versions, List<VersionsDTO>>>();
        var result = await handler.ExecuteAsync(new DefaultHttpContext(), new());

        result
            .Should()
            .BeEquivalentTo(
                new List<VersionsDTO>
                {
                    new VersionsDTO
                    {
                        Platform = PlatformDTO.IOS,
                        MinimumRequiredVersion = MinimumRequiredVersion.ToString(),
                        CurrentlySupportedVersion = CurrentlySupportedVersion.ToString(),
                    },
                    new VersionsDTO
                    {
                        Platform = PlatformDTO.Android,
                        MinimumRequiredVersion = MinimumRequiredVersion.ToString(),
                        CurrentlySupportedVersion = CurrentlySupportedVersion.ToString(),
                    }
                }
            );
    }

    [Fact]
    public async Task Version_smaller_then_minimum_required_is_not_supported()
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<VersionSupport, VersionSupportDTO?>>();
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.IOS, Version = "0.9", }
        );

        result.Should().Be(VersionSupportDTO.UpdateRequired);
    }

    [Fact]
    public async Task Update_is_suggested_for_version_between_minium_and_current()
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<VersionSupport, VersionSupportDTO?>>();
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.IOS, Version = "1.2", }
        );

        result.Should().Be(VersionSupportDTO.UpdateSuggested);
    }

    [Fact]
    public async Task Version_above_currently_supported_is_up_to_date()
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<VersionSupport, VersionSupportDTO?>>();
        var result = await handler.ExecuteAsync(
            new DefaultHttpContext(),
            new VersionSupport { Platform = PlatformDTO.IOS, Version = "1.4", }
        );

        result.Should().Be(VersionSupportDTO.UpToDate);
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
