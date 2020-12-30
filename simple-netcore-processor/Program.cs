using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;


namespace simple_netcore_processor
{
    public class Program
   {
        public static void Main(string[] args)
        {

            var config = new ConfigurationBuilder()
                //.SetBasePath(Directory.GetCurrentDirectory())
                .SetBasePath("/app/build/PublishOutput")
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .Build();

            var host = new WebHostBuilder()
                //.UseContentRoot(Directory.GetCurrentDirectory())
                .UseContentRoot("/app/build/PublishOutput")
                .UseConfiguration(config)
                .UseStartup<Startup>()
                .UseKestrel(
                    options =>
                    {
                        options.AllowSynchronousIO = true;
                        options.Limits.MaxConcurrentConnections = 100000;
                        options.Limits.MaxConcurrentUpgradedConnections = 100000;
                        options.Limits.MinRequestBodyDataRate = null;
                        options.Limits.MinResponseDataRate = null;
                        options.Limits.MinRequestBodyDataRate = new MinDataRate(bytesPerSecond: 100,
                            gracePeriod: TimeSpan.FromSeconds(2));
                        options.Limits.MinResponseDataRate =
                            new MinDataRate(bytesPerSecond: 100,
                                gracePeriod: TimeSpan.FromSeconds(2));
                        options.AddServerHeader = false;
                        options.Listen(IPAddress.Any, 8080, listenOptions =>
                        {
                            listenOptions.UseConnectionLogging();

                        });
                    })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                    logging.AddDebug();
                })

                .Build();
            host.Run();
        }
    }
}
