using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Serilog;
using Serilog.Core;

namespace LeanCode.Components.Startup
{
    internal static class SerilogExtensions
    {
        internal static LoggerConfiguration DestructureCommonObjects(
            this LoggerConfiguration config,
            Assembly[]? searchAssemblies)
        {
            if (searchAssemblies != null)
            {
                return SelectTypes<IDestructuringPolicy>(searchAssemblies)
                    .Aggregate(config, (a, s) => a.Destructure.With(s));
            }
            else
            {
                return config;
            }
        }

        [Obsolete]
        internal static LoggerConfiguration EnrichWithAppName(
            this LoggerConfiguration config,
            string appName)
        {
            return config.Enrich.WithProperty("AppName", appName);
        }

        private static List<TType?> SelectTypes<TType>(Assembly[] searchAssemblies)
        {
            return searchAssemblies
                .SelectMany(a => a.ExportedTypes)
                .Where(t =>
                    typeof(TType).IsAssignableFrom(t) &&
                    t.IsPublic &&
                    t.GetConstructor(Type.EmptyTypes) != null)
                .Select(Activator.CreateInstance)
                .Cast<TType>()
                .ToList();
        }
    }
}
