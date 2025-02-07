using System.Globalization;
using System.Resources;
using Microsoft.Extensions.Logging;
using static System.Globalization.CultureInfo;

namespace LeanCode.Localization.StringLocalizers;

public class ResourceManagerStringLocalizer : IStringLocalizer
{
    private readonly ILogger<ResourceManagerStringLocalizer> logger;

    private readonly ResourceManager resourceManager;

    public ResourceManagerStringLocalizer(ILogger<ResourceManagerStringLocalizer> logger, LocalizationConfiguration cfg)
    {
        this.logger = logger;
        resourceManager = new ResourceManager(cfg.ResourceSource);
    }

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1065", Justification = "Expected behavior.")]
    public string this[CultureInfo culture, string name]
    {
        get
        {
            logger.LogDebug(
                "Retrieving string {Name} for culture {Culture}",
                name,
                culture.Name.Length == 0 ? nameof(InvariantCulture) : culture.Name
            );

            try
            {
                var value = resourceManager.GetString(name, culture);

                return value ?? throw new InvalidOperationException("Name cannot be found in a resource set.");
            }
            catch (Exception e)
                when (e is InvalidOperationException
                    || e is MissingManifestResourceException
                    || e is MissingSatelliteAssemblyException
                )
            {
#pragma warning disable CA2254
                logger.LogError(e, e.Message.TrimEnd('.'));
#pragma warning restore CA2254

                throw new LocalizedResourceNotFoundException(e);
            }
        }
    }
}
