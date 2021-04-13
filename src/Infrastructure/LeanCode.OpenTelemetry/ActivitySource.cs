using System.Diagnostics;

namespace LeanCode.OpenTelemetry
{
    public static class LeanCodeActivitySource
    {
        public static readonly ActivitySource ActivitySource = new("LeanCode.CoreLibrary");
        public static Activity? Start(string name) => ActivitySource.StartActivity(name);
    }
}
