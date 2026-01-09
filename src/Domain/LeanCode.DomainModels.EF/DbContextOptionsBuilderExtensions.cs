using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.EF;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Automatically registers TypeMappingPlugin for all TypedIds.
    /// Supports types implementing IPrefixedTypedId and IRawTypedId with any backing type (int, long, Guid).
    /// </summary>
    public static DbContextOptionsBuilder AddAllTypedIdPlugins(this DbContextOptionsBuilder builder)
    {
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(new TypedIdDbContextOptionsExtension());

        return builder;
    }
}

internal class TypedIdDbContextOptionsExtension : IDbContextOptionsExtension
{
    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton<IRelationalTypeMappingSourcePlugin, TypedIdTypeMappingSourcePlugin>();
    }

    public void Validate(IDbContextOptions options) { }

    private sealed class ExtensionInfo(IDbContextOptionsExtension extension) : DbContextOptionsExtensionInfo(extension)
    {
        public override bool IsDatabaseProvider => false;

        public override string LogFragment => "TypedIdPlugin";

        public override int GetServiceProviderHashCode() => 0;

        public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other)
        {
            return other is ExtensionInfo;
        }

        public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
        {
            debugInfo["TypedId:Enabled"] = "1";
        }
    }
}
