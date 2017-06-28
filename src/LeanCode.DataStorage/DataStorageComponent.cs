using System;
using Autofac.Core;
using AutoMapper;
using LeanCode.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DataStorage
{
    public class DataStorageComponent : IAppComponent
    {
        public IModule AutofacModule { get; }
        public Profile MapperProfile { get; }


        [Obsolete("Use WithConfiguration/WithoutConfiguration factory methods.")]
        public DataStorageComponent(IConfiguration config)
            : this(config, true)
        { }

        private DataStorageComponent(IConfiguration config, bool useConfig)
        {
            if (useConfig && config == null)
            {
                throw new ArgumentNullException("Provide config when using configuration.", nameof(config));
            }
            else if (!useConfig && config != null)
            {
                throw new ArgumentNullException("Do not provide config, when config is not used.", nameof(config));
            }
            AutofacModule = new DataStorageModule(config);
        }

        public void ConfigureServices(IServiceCollection services)
        { }

        public static DataStorageComponent WithoutConfiguration() => new DataStorageComponent(null, false);
        public static DataStorageComponent WithConfiguration(IConfiguration config) => new DataStorageComponent(config, true);
    }
}
