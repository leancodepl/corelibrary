using System.Net;
using System.Text.Json;
using LeanCode.ForceUpdate.Contracts;
using FluentAssertions;
using Xunit;

namespace LeanCode.ForceUpdate.Tests;

public class ForceUpdateTests : ForceUpdateTestsBase
{
    [Fact]
    public async Task Versions_correctly_returns_all_versions()
    {
        var (body, statusCode) = await SendAsync("/cqrs/query/LeanCode.ForceUpdate.Contracts.Versions");

        statusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonSerializer.Deserialize<List<VersionsDTO>>(body);
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
        var (body, statusCode) = await SendAsync(
            "/cqrs/query/LeanCode.ForceUpdate.Contracts.VersionSupport",
            @"{
                ""Platform"": 0,
                ""Version"": ""0.9""
            }"
        );

        statusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonSerializer.Deserialize<VersionSupportDTO?>(body);
        result.Should().Be(VersionSupportDTO.UpdateRequired);
    }

    [Fact]
    public async Task Update_is_suggested_for_version_between_minium_and_current()
    {
        var (body, statusCode) = await SendAsync(
            "/cqrs/query/LeanCode.ForceUpdate.Contracts.VersionSupport",
            @"{
                ""Platform"": 0,
                ""Version"": ""1.2""
            }"
        );

        statusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonSerializer.Deserialize<VersionSupportDTO?>(body);
        result.Should().Be(VersionSupportDTO.UpdateSuggested);
    }

    [Fact]
    public async Task Version_above_currently_supported_is_up_to_date()
    {
        var (body, statusCode) = await SendAsync(
            "/cqrs/query/LeanCode.ForceUpdate.Contracts.VersionSupport",
            @"{
                ""Platform"": 0,
                ""Version"": ""1.4""
            }"
        );

        statusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonSerializer.Deserialize<VersionSupportDTO?>(body);
        result.Should().Be(VersionSupportDTO.UpToDate);
    }

    [Fact]
    public async Task Version_support_returns_null_for_not_supported_platform()
    {
        var (body, statusCode) = await SendAsync(
            "/cqrs/query/LeanCode.ForceUpdate.Contracts.VersionSupport",
            @"{
                ""Platform"": 4,
                ""Version"": ""1.4""
            }"
        );

        statusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonSerializer.Deserialize<VersionSupportDTO?>(body);
        result.Should().BeNull();
    }

    [Fact]
    public async Task Version_support_returns_null_for_invalid_version()
    {
        var (body, statusCode) = await SendAsync(
            "/cqrs/query/LeanCode.ForceUpdate.Contracts.VersionSupport",
            @"{
                ""Platform"": 0,
                ""Version"": ""1.x""
            }"
        );

        statusCode.Should().Be(HttpStatusCode.OK);
        var result = JsonSerializer.Deserialize<VersionSupportDTO?>(body);
        result.Should().BeNull();
    }
}