using Autofac;
using FluentValidation;

namespace LeanCode.CQRS.Validation.Fluent
{
    public static class ValidationContextExtensions
    {
        public const string ComponentContextKey = "ComponentContext";
        public const string AppContextKey = "AppContext";

        public static TAppContext? AppContext<TAppContext>(this ValidationContext ctx)
            where TAppContext : class
        {
            return ctx.RootContextData.TryGetValue(AppContextKey, out var ac)
                ? (TAppContext)ac
                : null;
        }

        public static T? GetService<T>(this ValidationContext ctx)
            where T : class
        {
            return ctx.RootContextData.TryGetValue(ComponentContextKey, out var cc)
                ? ((IComponentContext)cc).Resolve<T>()
                : null;
        }
    }
}
