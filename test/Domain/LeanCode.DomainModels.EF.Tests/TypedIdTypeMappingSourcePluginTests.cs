using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace LeanCode.DomainModels.EF.Tests;

public class TypedIdTypeMappingSourcePluginTests
{
    private readonly TypedIdTypeMappingSourcePlugin postgresPlugin;
    private readonly TypedIdTypeMappingSourcePlugin sqlServerPlugin;

    public TypedIdTypeMappingSourcePluginTests()
    {
        postgresPlugin = new TypedIdTypeMappingSourcePlugin(new PostgresTypedIdStoreTypeProvider());
        sqlServerPlugin = new TypedIdTypeMappingSourcePlugin(new SqlServerTypedIdStoreTypeProvider());
    }

    [Theory]
    [InlineData(typeof(PrefixedGuidId), "citext", "nchar(45)")]
    [InlineData(typeof(PrefixedUlidId), "citext", "nchar(39)")]
    [InlineData(typeof(PrefixedStringId), "citext", "nchar(115)")]
    public void Returns_correct_mapping_for_prefixed_typed_ids(
        Type idType,
        string expectedPostgres,
        string expectedSqlServer
    )
    {
        AssertMapping(postgresPlugin, idType, expectedPostgres);
        AssertMapping(sqlServerPlugin, idType, expectedSqlServer);
    }

    [Theory]
    [InlineData(typeof(IntId), "integer", "int")]
    [InlineData(typeof(LongId), "bigint", "bigint")]
    [InlineData(typeof(GuidId), "uuid", "uniqueidentifier")]
    public void Returns_correct_mapping_for_raw_typed_ids(
        Type idType,
        string expectedPostgres,
        string expectedSqlServer
    )
    {
        AssertMapping(postgresPlugin, idType, expectedPostgres);
        AssertMapping(sqlServerPlugin, idType, expectedSqlServer);
    }

    [Theory]
    [InlineData(typeof(StringId), "citext", "nvarchar(100)")]
    public void Returns_correct_mapping_for_raw_string_typed_ids(
        Type idType,
        string expectedPostgres,
        string expectedSqlServer
    )
    {
        AssertMapping(postgresPlugin, idType, expectedPostgres);
        AssertMapping(sqlServerPlugin, idType, expectedSqlServer);
    }

    [Fact]
    public void Returns_null_for_non_typed_id_types()
    {
        var mappingInfo = new RelationalTypeMappingInfo(typeof(int));

        Assert.Null(postgresPlugin.FindMapping(mappingInfo));
        Assert.Null(sqlServerPlugin.FindMapping(mappingInfo));
    }

    [Fact]
    public void Returns_null_for_null_type()
    {
        var mappingInfo = new RelationalTypeMappingInfo();

        Assert.Null(postgresPlugin.FindMapping(mappingInfo));
        Assert.Null(sqlServerPlugin.FindMapping(mappingInfo));
    }

    private static void AssertMapping(TypedIdTypeMappingSourcePlugin plugin, Type idType, string expectedStoreType)
    {
        var mappingInfo = new RelationalTypeMappingInfo(idType);
        var mapping = plugin.FindMapping(mappingInfo);

        Assert.NotNull(mapping);
        Assert.Equal(idType, mapping.ClrType);
        Assert.Equal(expectedStoreType, mapping.StoreType);
    }
}
