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
            if (cfg is null)
            {
                throw new ArgumentNullException(nameof(cfg));
            }

            this.resourceManager = new ResourceManager(cfg.ResourceSource);
        }

        /// <inheritdoc />
        public string this[CultureInfo culture, string name]
        {
            get
            {
                if (culture is null)
                {
                    throw new ArgumentNullException(nameof(culture));
                }

                if (name is null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                logger.Verbose("Retrieving string {Name} for culture {Culture}",
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

        /// <inheritdoc />
        public string this[CultureInfo culture, string name, params object[] arguments]
        {
            get
            {
                if (culture is null)
                {
                    throw new ArgumentNullException(nameof(culture));
                }

                if (name is null)
                {
                    throw new ArgumentNullException(nameof(name));
                }

                if (arguments is null)
                {
                    throw new ArgumentNullException(nameof(arguments));
                }

                logger.Verbose("Retrieving string {Name} for culture {Culture} and formatting with {NArgs} arguments",
                    name, culture.Name.Length == 0 ? "InvariantCulture" : culture.Name, arguments.Length);

                try
                {
                    string format = resourceManager.GetString(name, culture);

                    return string.Format(
                        culture,
                        format ?? throw new InvalidOperationException(
                            "Name cannot be found in a resource set."),
                        arguments);
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
