using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Collections.Generic;
using Larva.RaftAlgo.Concensus.Node;
using Larva.RaftAlgo;

namespace BusinessCodeGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            const string promopt = @"Usage: ./BusinessCodeGenerator <option>
Option:
\t --api-url=<api_url>
\t --node-id=<node_id>
\t --node-url=<node_url>
";

            var commandLineProvider = new CommandLineConfigurationProvider(args, new Dictionary<string, string>
            {
                {"--api-url", "api_url"},
                {"--node-id", "node_id"},
                {"--node-url", "node_url"}
            });
            commandLineProvider.Load();

            if (!commandLineProvider.TryGet("api_url", out string apiUrl))
            {
                Console.WriteLine(promopt);
                Console.WriteLine("Please input --api-url=<api_url>");
                return;
            }

            if (!commandLineProvider.TryGet("node_id", out string nodeId))
            {
                nodeId = Environment.MachineName;
            }

            if (!commandLineProvider.TryGet("node_url", out string nodeUrl))
            {
                nodeUrl = apiUrl;
            }

            var webHostBuilder = new WebHostBuilder();
            webHostBuilder.UseUrls(apiUrl == nodeUrl ? new string[] { apiUrl } : new string[] { apiUrl, nodeUrl })
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                    config.SetBasePath(hostingContext.HostingEnvironment.ContentRootPath);
                    var env = hostingContext.HostingEnvironment;
                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: false);
                    config.AddJsonFile("peers.json", optional: true, reloadOnChange: false);
                    config.AddCommandLine(args);
                })

                .ConfigureLogging(logging => { logging.AddLog4Net("log4net.config", true); })
                .ConfigureServices(x =>
                {
                    x.AddSingleton<NodeId>(new NodeId(nodeId, new Uri(nodeUrl)));
                    x.AddSingleton<IRaftSettings>(new InMemoryRaftSettings
                    {
                        MinElectionTimeoutMilliseconds = 5000,
                        MaxElectionTimeoutMilliseconds = 10000
                    });

                    x.AddRouting();
                    x.AddRaftAlgo();
                })
                .Configure(app =>
                {
                    app.UseRaftAlgo();

                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/", async context => { await context.Response.WriteAsync("ok"); });
                    });
                });

            var webHost = webHostBuilder.Build();
            webHost.Start();
            Console.WriteLine($"Node {nodeId} start at {apiUrl}");
            webHost.WaitForShutdown();
            Console.WriteLine($"Node {nodeId} shutdown!");
        }
    }
}
