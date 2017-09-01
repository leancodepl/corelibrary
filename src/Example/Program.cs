using System.IO;
using LeanCode.Components.Startup;
using Microsoft.AspNetCore.Hosting;

namespace LeanCode.Example
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = LeanProgram.BuildDefaultWebHost<Startup>()
                .Build();

            host.Run();
        }
    }
}
