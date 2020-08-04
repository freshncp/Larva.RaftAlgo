using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BusinessCodeGenerator.Caches;
using BusinessCodeGenerator.Configuration;
using Larva.RaftAlgo.Concensus.Node;
using Larva.RaftAlgo.Concensus.Rpc.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BusinessCodeGenerator.Raft
{
    public class HttpRpcServer
    {
        private readonly INode _node;
        private readonly ILogger _logger;
        private readonly IBusinessCodeCache _businessCodeCache;
        private readonly IBusinessCodeConfigManager _businessCodeConfigManager;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        public HttpRpcServer(INode node, IBusinessCodeCache businessCodeCache, IBusinessCodeConfigManager businessCodeConfigManager, ILoggerFactory loggerFactory)
        {
            _node = node;
            _logger = loggerFactory.CreateLogger("HttpRpcServer");
            _businessCodeCache = businessCodeCache;
            _businessCodeConfigManager = businessCodeConfigManager;
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        public async Task GetStatusAsync(HttpContext context)
        {
            var json = JsonConvert.SerializeObject(new
            {
                _node.Id,
                _node.ServiceUri,
                Role = _node.Role.ToString(),
                _node.State
            }, Formatting.Indented);
            await context.Response.WriteAsync(json);
        }

        public async Task ProcessRequestVoteAsync(HttpContext context)
        {
            try
            {
                string json;
                using (var reader = new StreamReader(context.Request.Body))
                {
                    json = await reader.ReadToEndAsync();
                }

                var requestVote =
                    JsonConvert.DeserializeObject<RequestVoteRequest>(json);
                _logger.LogDebug(
                    $"{context.Request.Path} called, my state is {_node.State.GetType().FullName}");
                var requestVoteResponse = await _node.RequestVoteAsync(requestVote);
                await context.Response.WriteAsync(
                    JsonConvert.SerializeObject(requestVoteResponse));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ex));
                _logger.LogError($"THERE WAS A PROBLEM ON NODE {_node.Id}", ex);
            }
        }

        public async Task ProcessAppendEntriesAsync(HttpContext context)
        {
            try
            {
                string json;
                using (var reader = new StreamReader(context.Request.Body))
                {
                    json = await reader.ReadToEndAsync();
                }

                var appendEntries =
                    JsonConvert.DeserializeObject<AppendEntriesRequest>(json);
                _logger.LogDebug(
                    $"{context.Request.Path} called, my state is {_node.State.GetType().FullName}");

                var appendEntriesResponse = await _node.AppendEntriesAsync(appendEntries);
                await context.Response.WriteAsync(
                    JsonConvert.SerializeObject(appendEntriesResponse));
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ex));
                _logger.LogError($"THERE WAS A PROBLEM ON NODE {_node.State}", ex);
            }
        }

        public async Task ProcessCommandAsync(HttpContext context)
        {
            try
            {
                string json;
                using (var reader = new StreamReader(context.Request.Body))
                {
                    json = await reader.ReadToEndAsync();
                }

                if (JsonConvert.DeserializeObject(json, _jsonSerializerSettings) is GenerateBusinessCodeCommand command)
                {
                    _logger.LogDebug($"{context.Request.Path} called, my state is {_node.State.GetType().FullName}");
                    if (_node.Role == NodeRole.Leader || command.CurrentTime == null)
                    {
                        var config = _businessCodeConfigManager.Get(command.ApplicationId, command.CodeType);
                        if (config == null)
                        {
                            await context.Response.WriteAsync(JsonConvert.SerializeObject(new ExecuteCommandResponse(null, false, "unable to save business code to state machine")));
                            return;
                        }
                        var code = _businessCodeCache.Get(command.ApplicationId, command.CodeType);
                        command = new GenerateBusinessCodeCommand(command.ApplicationId, command.CodeType, DateTime.Now);
                    }
                    var commandResponse = await _node.ExecuteCommandAsync(command);
                    await context.Response.WriteAsync(JsonConvert.SerializeObject(commandResponse));
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync("Bad command request.");
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(ex));
                _logger.LogError($"THERE WAS A PROBLEM ON NODE {_node.Id}", ex);
            }
        }
    }
}