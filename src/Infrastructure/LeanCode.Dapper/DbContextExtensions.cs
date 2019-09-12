using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.Dapper
{
    [SuppressMessage(
        "StyleCop.CSharp.LayoutRules",
        "SA1507:CodeMustNotContainMultipleBlankLinesInARow",
        Justification = "Grouping of the methods improves readability.")]
    public static class DbContextDapperExtensions
    {
        public static async Task<TResult> WithConnection<TResult>(
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
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType));
        }


        public static Task<T> ExecuteScalarAsync<T>(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType));
        }


        public static Task<IEnumerable<TResult>> QueryAsync<TResult>(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QueryAsync<TResult>(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<IEnumerable<TResult>> QueryAsync<TFirst, TSecond, TResult>(
            this DbContext context,
            string sql,
            Func<TFirst, TSecond, TResult> map,
            object? param = null,
            string splitOn = "Id",
            IDbTransaction? transaction = null,
            bool buffered = true,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QueryAsync(sql, map, param, transaction, buffered, splitOn, commandTimeout, commandType));
        }

        public static Task<IEnumerable<dynamic>> QueryAsync(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QueryAsync(sql, param, transaction, commandTimeout, commandType));
        }


        public static Task<TResult> QuerySingleOrDefaultAsync<TResult>(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QuerySingleOrDefaultAsync<TResult>(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<dynamic> QuerySingleOrDefaultAsync(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QuerySingleOrDefaultAsync(sql, param, transaction, commandTimeout, commandType));
        }


        public static Task<TResult> QuerySingleAsync<TResult>(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QuerySingleAsync<TResult>(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<dynamic> QuerySingleAsync(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QuerySingleAsync(sql, param, transaction, commandTimeout, commandType));
        }


        public static Task<TResult> QueryFirstOrDefaultAsync<TResult>(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QueryFirstOrDefaultAsync<TResult>(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<dynamic> QueryFirstOrDefaultAsync(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QueryFirstOrDefaultAsync(sql, param, transaction, commandTimeout, commandType));
        }


        public static Task<TResult> QueryFirstAsync<TResult>(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QueryFirstAsync<TResult>(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<dynamic> QueryFirstAsync(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QueryFirstAsync(sql, param, transaction, commandTimeout, commandType));
        }


        public static Task<SqlMapper.GridReader> QueryMultipleAsync(
            this DbContext context,
            string sql,
            object? param = null,
            IDbTransaction? transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn =>
                conn.QueryMultipleAsync(sql, param, transaction, commandTimeout, commandType));
        }
    }
}
