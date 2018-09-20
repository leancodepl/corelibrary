using System;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LeanCode.Dapper
{
    // Inspired by QueryValidator.Fody
    // https://github.com/kamil-mrzyglod/QueryValidator.Fody/blob/master/QueryValidator.Fody/QueryValidator.Fody/ModuleWeaver.cs
    public static class RawQueryValidator
    {
        public static async Task ValidateQueries(string connectionString, params Assembly[] assemblies)
        {
            var dapperSqls = assemblies
                .SelectMany(a => a.DefinedTypes)
                .SelectMany(t => t.DeclaredFields)
                .Where(f => f.IsDefined(typeof(RawSqlQueryAttribute), false))
                .Where(f => f.IsStatic && f.FieldType == typeof(string));

            using (var connection = new SqlConnection(connectionString))
            {
                await connection.OpenAsync();

                foreach (var sql in dapperSqls)
                {
                    using (var command = new SqlCommand())
                    {
                        try
                        {
                            var sqlText = (string)sql.GetValue(null);
                            sqlText = Regex.Replace(sqlText, "IN @[a-zA-Z]{0,}", "IN (1)", RegexOptions.Compiled);
                            sqlText = Regex.Replace(sqlText, "(?i)(?<!DECLARE\\s+)@[a-zA-Z0-9]{0,}", "''", RegexOptions.Compiled);
                            // https://xkcd.com/208/

                            command.Connection = connection;
                            command.CommandText = $"SET FMTONLY ON; {sqlText}";
                            await command.ExecuteNonQueryAsync();
                        }
                        catch (SqlException e)
                        {
                            throw new Exception($"Sql server could not process sql query from `{sql.Name}` in {sql.DeclaringType.FullName}\n{e.Message}", e);
                        }
                    }
                }
            }
        }
    }
}
