using LeanCode.Components.Startup;
using Microsoft.AspNetCore.Hosting;

namespace LeanCode.Example
{
    public class Program
    {
        public static void Main()
        {
            LeanProgram.BuildMinimalWebHost<Startup>()
                .ConfigureDefaultLogging("example")
                .UseKestrel()
                .Build()
                .Run();
        }
    }
}
