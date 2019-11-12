using Serilog;

namespace LeanCode.IntegrationTestHelpers
{
    public static class IntegrationTestLogging
    {
        static IntegrationTestLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Warning()
                .WriteTo.Console()
                .CreateLogger();
        }
    }
}
