using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog;
using Serilog.Core;

namespace LeanCode.Components.Startup
{
    static class SerilogExtensions
    {
        public static LoggerConfiguration DestructureCommonObjects(
            this LoggerConfiguration config,
            Assembly[] searchAssemblies)
        {
            var policies = SelectTypes<IDestructuringPolicy>(searchAssemblies);
            return policies.Aggregate(config, (a, s) => a.Destructure.With(s));
        }

        public static LoggerConfiguration EnrichWithAppName(
            this LoggerConfiguration config,
            string appName)
        {
            return config.Enrich.WithProperty("AppName", appName);
        }

        private static List<TType> SelectTypes<TType>(Assembly[] searchAssemblies)
        {
            return
                searchAssemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t =>
                    typeof(TType).IsAssignableFrom(t) &&
                    t.GetTypeInfo().IsPublic &&
                    t.GetTypeInfo().GetConstructor(Type.EmptyTypes) != null)
                .Select(Activator.CreateInstance)
                .Cast<TType>()
                .ToList();
        }
    }
}
