using System.Diagnostics;

namespace LeanCode.OpenTelemetry;

public static class LeanCodeActivitySource
{
    public static readonly ActivitySource ActivitySource = new("LeanCode.CoreLibrary");

    public static Activity? Start(string name) => ActivitySource.StartActivity(name);

    public static Activity? StartExecution(string type, string? objectType) => Start($"{type} - {objectType}");

    public static Activity? StartMiddleware(string middlewareName) => Start($"middleware - {middlewareName}");

    public static Activity? StartMiddleware(string middlewareName, string? objectName) =>
        Start($"middleware - {middlewareName} - {objectName}");
}
