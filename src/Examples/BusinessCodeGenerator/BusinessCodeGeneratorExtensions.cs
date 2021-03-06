using System.Reflection;
using System.Threading.Tasks;
using BusinessCodeGenerator.Caches;
using BusinessCodeGenerator.Configuration;
using BusinessCodeGenerator.Raft;
using Larva.RaftAlgo;
using Larva.RaftAlgo.Concensus;
using Larva.RaftAlgo.Concensus.Cluster;
using Larva.RaftAlgo.Concensus.Node;
using Larva.RaftAlgo.Concensus.Rpc;
using Larva.RaftAlgo.Log;
using Larva.RaftAlgo.StateMachine;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BusinessCodeGenerator
{
    public static class BusinessCodeGeneratorExtensions
    {
        public static IServiceCollection AddRaftAlgo(this IServiceCollection services)
        {
            var configuration = (ConfigurationRoot)services.BuildServiceProvider()
                .GetRequiredService<IConfiguration>();
            services.Configure<RaftClusterConfig>(configuration.GetSection("RaftCluster"));
            services.AddSingleton<IClusterSettings, FromConfigRaftClusterSettings>();
            services.AddSingleton<IElectionTimeoutRandom, DefaultElectionTimeoutRandom>();
            services.AddSingleton<ICommandSerializer, JsonCommandSerializer>();
            services.AddSingleton<ILog, SqlLiteLog>();
            services.AddSingleton<IRpcClientProvider, HttpRpcClientProvider>();
            services.AddSingleton<INode, LocalNode>();
            services.AddSingleton<ICluster, DefaultCluster>();
            services.AddSingleton<IReplicatedStateMachine, BusinessCodeFiniteStateMachine>();
            services.AddSingleton<IBusinessCodeConfigManager, InMemoryBusinessCodeConfigManager>();
            services.AddSingleton<IBusinessCodeCache, BusinessCodeCache>();
            services.AddSingleton<HttpRpcServer>();
            return services;
        }

        public static IApplicationBuilder UseRaftAlgo(this IApplicationBuilder app, Assembly[] assemblies)
        {
            var commandSerializer = app.ApplicationServices.GetService<ICommandSerializer>();
            ((JsonCommandSerializer)commandSerializer).Initialize(assemblies);
            var applicationLifetime = app.ApplicationServices.GetService<IHostApplicationLifetime>();
            applicationLifetime.ApplicationStopping.Register(async () => await OnShutdown(app));
            var node = app.ApplicationServices.GetService<INode>() as LocalNode;
            var cluster = app.ApplicationServices.GetService<ICluster>();
            cluster.Load();
            node.StartUpAsync(cluster).Wait();

            app.UseRouting();
            var raftHttpServer = app.ApplicationServices.GetService<HttpRpcServer>();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/_raft/state", raftHttpServer.GetStatusAsync);
                endpoints.MapPost("/_raft/requestvote", raftHttpServer.ProcessRequestVoteAsync);
                endpoints.MapPost("/_raft/appendentries", raftHttpServer.ProcessAppendEntriesAsync);
                endpoints.MapPost("/_raft/command", raftHttpServer.ProcessCommandAsync);
            });
            return app;
        }

        private static async Task OnShutdown(IApplicationBuilder app)
        {
            var node = app.ApplicationServices.GetService<INode>() as LocalNode;
            await node.ShutdownAsync();
        }
    }
}