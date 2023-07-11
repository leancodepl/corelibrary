using Azure.Core;
using Azure.Identity;

namespace LeanCode.Azure.Tests;

public static class Env
{
    public const string TenantIdKey = "CORELIB_TESTS_TENANT_ID";
    public const string ClientIdKey = "CORELIB_TESTS_CLIENT_ID";
    public const string ClientSecretKey = "CORELIB_TESTS_CLIENT_SECRET";
    public const string NpgsqlConnectionStringKey = "CORELIB_TESTS_NPGSQL_CONNECTION_STRING";

    public static string SkipIfVariablesNotSet(params string[] variables)
    {
        return variables.Any(v => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(v)))
            ? $"Set `{string.Join(",", variables)}` env variables first"
            : null;
    }

    public static TokenCredential GetTokenCredential()
    {
        return new ClientSecretCredential(
            Environment.GetEnvironmentVariable(TenantIdKey),
            Environment.GetEnvironmentVariable(ClientIdKey),
            Environment.GetEnvironmentVariable(ClientSecretKey)
        );
    }
}
