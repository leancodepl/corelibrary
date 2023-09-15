using System.Collections.Concurrent;
using System.Text;
using MassTransit;
using MassTransit.EntityFrameworkCoreIntegration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace LeanCode.CQRS.MassTransitRelay.LockProviders;

// Vendoring SqlLockStatementProvider & friends
// MT implementation uses EF8 incompatible API
// https://github.com/MassTransit/MassTransit/blob/9e6c78573ad211a70b624fad31382faa331dc4d8/src/Persistence/MassTransit.EntityFrameworkIntegration/EntityFrameworkIntegration/SqlLockStatementProvider.cs
public class CustomSqlLockStatementProvider : ILockStatementProvider
{
    protected static readonly ConcurrentDictionary<Type, SchemaTableColumnTrio> TableNames = new();
    private readonly bool enableSchemaCaching;
    private readonly ILockStatementFormatter formatter;

    private string? DefaultSchema { get; }

    public CustomSqlLockStatementProvider(
        string defaultSchema,
        ILockStatementFormatter formatter,
        bool enableSchemaCaching = true
    )
    {
        DefaultSchema = defaultSchema;

        this.formatter = formatter;
        this.enableSchemaCaching = enableSchemaCaching;
    }

    public CustomSqlLockStatementProvider(ILockStatementFormatter formatter, bool enableSchemaCaching = true)
    {
        this.formatter = formatter;
        this.enableSchemaCaching = enableSchemaCaching;
    }

    public virtual string GetRowLockStatement<T>(DbContext context)
        where T : class
    {
        return FormatLockStatement<T>(context, nameof(ISaga.CorrelationId));
    }

    public virtual string GetRowLockStatement<T>(DbContext context, params string[] propertyNames)
        where T : class
    {
        return FormatLockStatement<T>(context, propertyNames);
    }

    public virtual string GetOutboxStatement(DbContext context)
    {
        var schemaTableTrio = GetSchemaAndTableNameAndColumnName(
            context,
            typeof(OutboxState),
            nameof(OutboxState.Created)
        );

        var sb = new StringBuilder(128);
        formatter.CreateOutboxStatement(
            sb,
            schemaTableTrio.Schema,
            schemaTableTrio.Table,
            schemaTableTrio.ColumnNames[0]
        );

        return sb.ToString();
    }

    private string FormatLockStatement<T>(DbContext context, params string[] propertyNames)
        where T : class
    {
        var schemaTableTrio = GetSchemaAndTableNameAndColumnName(context, typeof(T), propertyNames);

        var sb = new StringBuilder(128);
        formatter.Create(sb, schemaTableTrio.Schema, schemaTableTrio.Table);

        for (var i = 0; i < propertyNames.Length; i++)
        {
            formatter.AppendColumn(sb, i, schemaTableTrio.ColumnNames[i]);
        }

        formatter.Complete(sb);

        return sb.ToString();
    }

    private SchemaTableColumnTrio GetSchemaAndTableNameAndColumnName(
        DbContext context,
        Type type,
        params string[] propertyNames
    )
    {
        if (TableNames.TryGetValue(type, out var result) && enableSchemaCaching)
        {
            return result;
        }

        var entityType =
            context.Model.FindEntityType(type)
            ?? throw new InvalidOperationException($"Entity type not found: {TypeCache.GetShortName(type)}");

        var schema = entityType.GetSchema();
        var tableName =
            entityType.GetTableName() ?? throw new InvalidOperationException("Entity is not mapped to a table");

        var columnNames = new List<string>();

        foreach (var t in propertyNames)
        {
            var property = entityType.GetProperties().Single(x => x.Name.Equals(t, StringComparison.OrdinalIgnoreCase));
            var storeObjectIdentifier = StoreObjectIdentifier.Table(tableName, schema);
            var columnName =
                property.GetColumnName(storeObjectIdentifier)
                ?? throw new InvalidOperationException("Cannot get column name");
            ;

            columnNames.Add(columnName);
        }

        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new MassTransitException(
                $"Unable to determine saga table name: {TypeCache.GetShortName(type)} (using model metadata)."
            );
        }

        result = new SchemaTableColumnTrio(schema ?? DefaultSchema, tableName, columnNames.ToArray());

        if (enableSchemaCaching)
        {
            TableNames.TryAdd(type, result);
        }

        return result;
    }

    protected readonly record struct SchemaTableColumnTrio(
        string? Schema,
        string Table,
        IReadOnlyList<string> ColumnNames
    );
}
