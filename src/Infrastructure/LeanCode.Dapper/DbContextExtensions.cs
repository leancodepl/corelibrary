using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage;

namespace LeanCode.Dapper;

[SuppressMessage(
    "StyleCop.CSharp.LayoutRules",
    "SA1507:CodeMustNotContainMultipleBlankLinesInARow",
    Justification = "Grouping of the methods improves readability.")]
public static class DbContextDapperExtensions
{
    public static async Task<TResult> WithConnectionAsync<TResult>(
        this DbContext context,
        Func<DbConnection, Task<TResult>> call)
    {
        await context.Database.OpenConnectionAsync();
        var conn = context.Database.GetDbConnection();
        return await call(conn);
    }

    public static Task ExecuteAsync(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.ExecuteAsync(cmd));
    }


    public static Task<T> ExecuteScalarAsync<T>(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.ExecuteScalarAsync<T>(cmd));
    }


    public static Task<IEnumerable<TResult>> QueryAsync<TResult>(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.QueryAsync<TResult>(cmd));
    }

    public static Task<IEnumerable<TResult>> QueryAsync<TFirst, TSecond, TResult>(
        this DbContext context,
        string sql,
        Func<TFirst, TSecond, TResult> map,
        object? param = null,
        string splitOn = "Id",
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CommandFlags commandFlags = CommandFlags.Buffered,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, commandFlags, cancellationToken);
        return context.WithConnectionAsync(conn => conn.QueryAsync(cmd, map, splitOn));
    }

    public static Task<IEnumerable<dynamic>> QueryAsync(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.QueryAsync(cmd));
    }


    public static Task<TResult> QuerySingleOrDefaultAsync<TResult>(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.QuerySingleOrDefaultAsync<TResult>(cmd));
    }

    public static Task<dynamic> QuerySingleOrDefaultAsync(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.QuerySingleOrDefaultAsync(cmd));
    }


    public static Task<TResult> QuerySingleAsync<TResult>(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.QuerySingleAsync<TResult>(cmd));
    }

    public static Task<dynamic> QuerySingleAsync(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.QuerySingleAsync(cmd));
    }


    public static Task<TResult> QueryFirstOrDefaultAsync<TResult>(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.QueryFirstOrDefaultAsync<TResult>(cmd));
    }

    public static Task<dynamic> QueryFirstOrDefaultAsync(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.QueryFirstOrDefaultAsync(cmd));
    }


    public static Task<TResult> QueryFirstAsync<TResult>(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.QueryFirstAsync<TResult>(cmd));
    }

    public static Task<dynamic> QueryFirstAsync(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.QueryFirstAsync(cmd));
    }


    public static Task<SqlMapper.GridReader> QueryMultipleAsync(
        this DbContext context,
        string sql,
        object? param = null,
        IDbTransaction? transaction = null,
        int? commandTimeout = null,
        CommandType? commandType = null,
        CancellationToken cancellationToken = default)
    {
        var cmd = new CommandDefinition(sql, param, transaction, commandTimeout, commandType, cancellationToken: cancellationToken);
        return context.WithConnectionAsync(conn => conn.QueryMultipleAsync(cmd));
    }


    public static string GetFullTableName(this DbContext dbContext, Type entity)
    {
        var entityType = dbContext.Model.FindEntityType(entity);

        if (entityType is null)
        {
            throw new InvalidOperationException("Failed to find entity type.");
        }

        var table = entityType.GetTableName();

        if (table is null)
        {
            throw new InvalidOperationException("Entity is not mapped to database table.");
        }

        var helper = dbContext.GetService<ISqlGenerationHelper>();

        return entityType.GetSchema() is string schema
            ? $"{helper.DelimitIdentifier(schema)}.{helper.DelimitIdentifier(table)}"
            : helper.DelimitIdentifier(table);
    }

    public static string GetColumnName(this DbContext dbContext, Type entity, string propertyName)
    {
        var entityType = dbContext.Model.FindEntityType(entity);

        if (entityType is null)
        {
            throw new InvalidOperationException("Failed to find entity type.");
        }

        var table = entityType.GetTableName();

        if (table is null)
        {
            throw new InvalidOperationException("Entity is not mapped to database table.");
        }

        var storeObject = StoreObjectIdentifier.Table(table, entityType.GetSchema());

        var property = entityType.FindProperty(propertyName);

        if (property is null)
        {
            throw new InvalidOperationException($"Property with name '{propertyName}' is not defined.");
        }

        var column = property.FindColumn(in storeObject);

        if (column is null)
        {
            throw new InvalidOperationException($"Property with name '{propertyName}' is not mapped to a column.");
        }

        return dbContext.GetService<ISqlGenerationHelper>().DelimitIdentifier(column.Name);
    }
}
