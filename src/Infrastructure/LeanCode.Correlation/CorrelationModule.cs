using Autofac;
using LeanCode.Components;

namespace LeanCode.Correlation
{
    public class CorrelationModule : AppModule
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterGeneric(typeof(CorrelationElement<,,>))
                .AsSelf();
        }
    }
}
