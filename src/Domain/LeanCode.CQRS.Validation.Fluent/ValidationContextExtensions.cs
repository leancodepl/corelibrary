using System;
using Autofac;
using FluentValidation;

namespace LeanCode.CQRS.Validation.Fluent
{
    public static class ValidationContextExtensions
    {
        public const string ComponentContextKey = "ComponentContext";
        public const string AppContextKey = "AppContext";

        public static TAppContext AppContext<TAppContext>(this ValidationContext ctx)
            where TAppContext : class
        {
            if (ctx.RootContextData.TryGetValue(AppContextKey, out var ac))
            {
                return (TAppContext)ac;
            }
            else
            {
                throw new InvalidOperationException("Cannot use `AppContext` extension method outside the `ContextualValdiator` class.");
            }
        }

        public static T GetService<T>(this ValidationContext ctx)
            where T : class
        {
            if (ctx.RootContextData.TryGetValue(ComponentContextKey, out var cc))
            {
                return ((IComponentContext)cc).Resolve<T>();
            }
            else
            {
                throw new InvalidOperationException("Cannot use `GetService` extension method outside the `ContextualValdiator` class.");
            }
        }
    }
}
