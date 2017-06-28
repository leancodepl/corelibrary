using Autofac;
using LeanCode.Configuration;
using Microsoft.Extensions.Configuration;

namespace LeanCode.DataStorage
{
    class DataStorageModule : Module
    {
        private readonly IConfiguration config;

        public DataStorageModule(IConfiguration config)
        {
            this.config = config;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (config != null)
            {
                builder.ConfigSection<AzureStorageConfiguration>(config);
            }

            builder.RegisterType<AzureDataStorage>().As<IDataStorage>();
        }
    }
}
