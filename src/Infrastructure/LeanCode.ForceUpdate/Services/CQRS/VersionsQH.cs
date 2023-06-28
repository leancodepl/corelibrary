using LeanCode.CQRS.Execution;
using LeanCode.ForceUpdate.Contracts;
using Microsoft.AspNetCore.Http;

namespace LeanCode.ForceUpdate.Services.CQRS;

public class VersionsQH : IQueryHandler<Versions, List<VersionsDTO>>
{
    private readonly IOSVersionsConfiguration iOSConfiguration;
    private readonly AndroidVersionsConfiguration androidConfiguration;

    public VersionsQH(IOSVersionsConfiguration iOSConfiguration, AndroidVersionsConfiguration androidConfiguration)
    {
        this.iOSConfiguration = iOSConfiguration;
        this.androidConfiguration = androidConfiguration;
    }

    public Task<List<VersionsDTO>> ExecuteAsync(HttpContext context, Versions query)
    {
        return Task.FromResult(
            new List<VersionsDTO>
            {
                new VersionsDTO
                {
                    Platform = PlatformDTO.IOS,
                    MinimumRequiredVersion = iOSConfiguration.MinimumRequiredVersion.ToString(),
                    CurrentlySupportedVersion = iOSConfiguration.CurrentlySupportedVersion.ToString(),
                },
                new VersionsDTO
                {
                    Platform = PlatformDTO.Android,
                    MinimumRequiredVersion = androidConfiguration.MinimumRequiredVersion.ToString(),
                    CurrentlySupportedVersion = androidConfiguration.CurrentlySupportedVersion.ToString(),
                }
            }
        );
    }
}
