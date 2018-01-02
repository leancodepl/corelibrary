using System.IO;
using LeanCode.Components.Startup;
using Microsoft.AspNetCore.Hosting;

namespace LeanCode.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            LeanProgram.BuildDefaultWebHost<Startup>()
                .UseKestrel()
                .Build()
                .Run();
        }
    }
}
