using Autofac;
using LeanCode.Components;

namespace LeanCode.Facebook
{
    public class FacebookModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<FacebookClient>().AsSelf();
        }
    }
}
