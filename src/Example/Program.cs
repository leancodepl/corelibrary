using LeanCode.Components.Startup;
using Microsoft.AspNetCore.Hosting;

namespace LeanCode.Example
{
    public class Program
    {
        public static void Main()
        {
#pragma warning disable CS0612
            LeanProgram.BuildDefaultWebHost<Startup>("LeanCode.Example")
#pragma warning restore CS0612
                .UseKestrel()
                .Build()
                .Run();
        }
    }
}
