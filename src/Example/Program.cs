using System.IO;
using LeanCode.Components.Startup;
using Microsoft.AspNetCore.Hosting;

namespace LeanCode.Example
{
    public class Program
    {
        public static void Main()
        {
            LeanProgram.BuildDefaultWebHost<Startup>("LeanCode.Example")
                .UseKestrel()
                .Build()
                .Run();
        }
    }
}
