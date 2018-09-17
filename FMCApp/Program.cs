using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FMCApp.Helpers;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;

namespace FMCApp
{
    public class Program
    {
        private const string SeedArgs = "/seed";

        public static void Main(string[] args)
        {
            var seed = args.Any(x => x == SeedArgs);
            if (seed) args = args.Except(new[] { SeedArgs }).ToArray();

            var host = BuildWebHost(args);

            // Uncomment this to seed upon startup, alternatively pass in `dotnet run /seed` to seed using CLI
            //DbMigrationHelpers.EnsureSeedData(host).GetAwaiter().GetResult();
            if (seed)
            {
                DbMigrationHelpers.EnsureSeedData(host).GetAwaiter().GetResult();
            }

            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                   .UseKestrel(c => c.AddServerHeader = false)
                   .UseStartup<Startup>()
                   .UseSerilog()
                   .Build();
    }
}
