using Autofac;
using LeanCode.Configuration;
using Microsoft.Extensions.Configuration;

namespace LeanCode.Facebook
{
    class FacebookModule : Module
    {
        private readonly IConfiguration configuration;

        public FacebookModule(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (configuration != null)
            {
                builder.ConfigSection<FacebookConfiguration>(configuration);
            }

            builder.RegisterType<FacebookClient>().AsSelf();
        }
    }
}
