using Autofac;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace LeanCode.Configuration
{
    public static class ConfigurationExtensions
    {
        public static ContainerBuilder ConfigSection<TConfig>(this ContainerBuilder builder, IConfiguration config)
            where TConfig : class, new()
        {
            var section = config.GetSection(Name<TConfig>());
            builder.Register(_ => new ConfigureFromConfigurationOptions<TConfig>(section))
                .As<IConfigureOptions<TConfig>>();
            builder.Register(ctx => ctx.Resolve<IOptions<TConfig>>().Value).AsSelf();
            return builder;
        }

        public static TConfig Options<TConfig>(this IConfiguration config, string name = null)
            where TConfig : new()
        {
            var opts = new TConfig();
            config.GetSection(name ?? Name<TConfig>()).Bind(opts);
            return opts;
        }

        private static string Name<TConfig>()
        {
            const string configSuffix = "Configuration";

            var name = typeof(TConfig).Name;
            if (name.EndsWith(configSuffix))
            {
                name = name.Remove(name.Length - configSuffix.Length);
            }
            return name;
        }
    }
}
