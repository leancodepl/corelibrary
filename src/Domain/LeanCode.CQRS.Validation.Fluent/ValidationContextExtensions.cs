using Autofac;
using FluentValidation;

namespace LeanCode.CQRS.Validation.Fluent
{
    public static class ValidationContextExtensions
    {
        public const string ComponentContextKey = "ComponentContext";
        public const string AppContextKey = "AppContext";
        public const string ObjectContextKey = "ObjectContext";

        public static TAppContext AppContext<TAppContext>(this ValidationContext ctx)
        {
            if (ctx.RootContextData.TryGetValue(AppContextKey, out var ac))
            {
                return (TAppContext)ac;
            }
            else
            {
                return default(TAppContext);
            }
        }

        public static TContext ObjectContext<TContext>(this ValidationContext ctx)
        {
            if (ctx.RootContextData.TryGetValue(ObjectContextKey, out var ac))
            {
                return (TContext)ac;
            }
            else
            {
                return default(TContext);
            }
        }

        public static T GetService<T>(this ValidationContext ctx)
        {
            if (ctx.RootContextData.TryGetValue(ComponentContextKey, out var cc))
            {
                return ((IComponentContext)cc).Resolve<T>();
            }
            else
            {
                return default(T);
            }
        }
    }
}
