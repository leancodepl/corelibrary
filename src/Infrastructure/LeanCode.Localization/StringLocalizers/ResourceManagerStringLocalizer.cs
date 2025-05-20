using System.Globalization;
using System.Resources;
using LeanCode.Logging;
using static System.Globalization.CultureInfo;

namespace LeanCode.Localization.StringLocalizers;

public class ResourceManagerStringLocalizer : IStringLocalizer
{
    private readonly ResourceManager resourceManager;
    private readonly ILogger<ResourceManagerStringLocalizer> logger;

    public ResourceManagerStringLocalizer(LocalizationConfiguration cfg, ILogger<ResourceManagerStringLocalizer> logger)
    {
        resourceManager = new ResourceManager(cfg.ResourceSource);
        this.logger = logger;
    }

    /// <inheritdoc />
    [System.Diagnostics.CodeAnalysis.SuppressMessage("?", "CA1065", Justification = "Expected behavior.")]
    public string this[CultureInfo culture, string name]
    {
        get
        {
            logger.Verbose(
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
                logger.Error(e, e.Message.TrimEnd('.'));

                throw new LocalizedResourceNotFoundException(e);
            }
        }
    }
}
