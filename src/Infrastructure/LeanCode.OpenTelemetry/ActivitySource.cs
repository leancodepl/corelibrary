using System.Diagnostics;

namespace LeanCode.OpenTelemetry
{
    public static class LeanCodeActivitySource
    {
        public static readonly ActivitySource ActivitySource = new ActivitySource("LeanCode.CoreLibrary");
        public static Activity? Start(string name) => ActivitySource.StartActivity(name);
    }
}
