using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;

namespace LeanCode.Dapper
{
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
            object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn => conn.ExecuteAsync(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<T> ExecuteScalarAsync<T>(
            this DbContext context,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn => conn.ExecuteScalarAsync<T>(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<IEnumerable<TResult>> QueryAsync<TResult>(
            this DbContext context,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn => conn.QueryAsync<TResult>(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<IEnumerable<dynamic>> QueryAsync(
            this DbContext context,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn => conn.QueryAsync(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<TResult> QuerySingleOrDefaultAsync<TResult>(
            this DbContext context,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn => conn.QuerySingleOrDefaultAsync<TResult>(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<dynamic> QuerySingleOrDefaultAsync(
            this DbContext context,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn => conn.QuerySingleOrDefaultAsync(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<TResult> QuerFirstOrDefaultAsync<TResult>(
            this DbContext context,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn => conn.QueryFirstOrDefaultAsync<TResult>(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<dynamic> QueryFirstOrDefaultAsync(
            this DbContext context,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn => conn.QueryFirstOrDefaultAsync(sql, param, transaction, commandTimeout, commandType));
        }

        public static Task<SqlMapper.GridReader> QueryMultipleAsync(
            this DbContext context,
            string sql,
            object param = null,
            IDbTransaction transaction = null,
            int? commandTimeout = null,
            CommandType? commandType = null)
        {
            return context.WithConnection(conn => conn.QueryMultipleAsync(sql, param, transaction, commandTimeout, commandType));
        }
    }
}
