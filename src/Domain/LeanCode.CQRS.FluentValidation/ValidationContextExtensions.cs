using Autofac;
using FluentValidation;

namespace LeanCode.CQRS.FluentValidation
{
    public static class ValidationContextExtensions
    {
        public const string ComponentContextKey = "ComponentContext";

        public static T GetService<T>(this ValidationContext ctx)
        {
            if (ctx.RootContextData.TryGetValue(ComponentContextKey, out var cc))
            {
                return ((IComponentContext)cc).Resolve<T>();
            }
            return default(T);
        }
    }
}
