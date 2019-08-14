using System;
using System.Globalization;
using System.Resources;

namespace LeanCode.Localization.StringLocalizers
{
    public class ResourceManagerStringLocalizer : IStringLocalizer
    {
        private readonly Serilog.ILogger logger = Serilog.Log.ForContext<ResourceManagerStringLocalizer>();

        private readonly ResourceManager resourceManager;

        public ResourceManagerStringLocalizer(LocalizationConfiguration cfg)
        {
            _ = cfg ?? throw new ArgumentNullException(nameof(cfg));

            this.resourceManager = new ResourceManager(cfg.ResourceSource);
        }

        /// <inheritdoc />
        public string this[CultureInfo culture, string name]
        {
            get
            {
                _ = culture ?? throw new ArgumentNullException(nameof(culture));
                _ = name ?? throw new ArgumentNullException(nameof(name));

                logger.Verbose(
                    "Retrieving string {Name} for culture {Culture}",
                    name, culture.Name.Length == 0 ? "InvariantCulture" : culture.Name);

                try
                {
                    string value = resourceManager.GetString(name, culture);

                    return value ?? throw new InvalidOperationException(
                        "Name cannot be found in a resource set.");
                }
                catch (Exception e) when (
                    e is InvalidOperationException ||
                    e is MissingManifestResourceException ||
                    e is MissingSatelliteAssemblyException)
                {
                    logger.Error(e, e.Message.TrimEnd('.'));
                    throw new LocalizedResourceNotFoundException(e);
                }
            }
        }
    }
}
