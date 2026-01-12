using LeanCode.DomainModels.Ids;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace LeanCode.DomainModels.EF;

public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Automatically registers TypeMappingPlugin for all TypedIds.
    /// Supports types implementing <see cref="IPrefixedTypedId{TSelf}"/>, <see cref="IRawStringTypedId{TSelf}"/> and <see cref="IRawTypedId{TBacking,TSelf}"/> with any backing type (int, long, Guid).
    /// </summary>
    public static DbContextOptionsBuilder AddPostgresTypedIdMappingPlugins(this DbContextOptionsBuilder builder)
    {
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(
            new TypedIdDbContextOptionsExtension(new PostgresTypedIdStoreTypeProvider())
        );

        return builder;
    }

    /// <summary>
    /// Automatically registers TypeMappingPlugin for all TypedIds.
    /// Supports types implementing <see cref="IPrefixedTypedId{TSelf}"/>, <see cref="IRawStringTypedId{TSelf}"/> and <see cref="IRawTypedId{TBacking,TSelf}"/> with any backing type (int, long, Guid).
    /// </summary>
    public static DbContextOptionsBuilder AddSqlServerTypedIdMappingPlugins(this DbContextOptionsBuilder builder)
    {
        ((IDbContextOptionsBuilderInfrastructure)builder).AddOrUpdateExtension(
            new TypedIdDbContextOptionsExtension(new SqlServerTypedIdStoreTypeProvider())
        );

        return builder;
    }
}

internal class TypedIdDbContextOptionsExtension : IDbContextOptionsExtension
{
    private readonly ITypedIdStoreTypeProvider storeTypeProvider;

    public TypedIdDbContextOptionsExtension(ITypedIdStoreTypeProvider storeTypeProvider)
    {
        this.storeTypeProvider = storeTypeProvider;
    }

    public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

    public void ApplyServices(IServiceCollection services)
    {
        services.AddSingleton<IRelationalTypeMappingSourcePlugin>(
            new TypedIdTypeMappingSourcePlugin(storeTypeProvider)
        );
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
