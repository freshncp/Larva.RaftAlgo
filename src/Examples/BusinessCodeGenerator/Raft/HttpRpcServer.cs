using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using BusinessCodeGenerator.Caches;
using Larva.RaftAlgo.Concensus.Node;
using Larva.RaftAlgo.Concensus.Rpc.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BusinessCodeGenerator.Raft
{
    public class HttpRpcServer
    {
        private INode _node;
        private ILogger _logger;
        private IBusinessCodeCache _businessCodeCache;
        private JsonSerializerSettings _jsonSerializerSettings;

        public HttpRpcServer(INode node, IBusinessCodeCache businessCodeCache, ILoggerFactory loggerFactory)
        {
            _node = node;
            _logger = loggerFactory.CreateLogger("HttpRpcServer");
            _businessCodeCache = businessCodeCache;
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
                    JsonConvert.DeserializeObject<RequestVoteRequest>(json, _jsonSerializerSettings);
                _logger.LogDebug(
                    $"{context.Request.Path} called, my state is {_node.State.GetType().FullName}");
                var requestVoteResponse = await _node.RequestVoteAsync(requestVote);
                await context.Response.WriteAsync(
                    JsonConvert.SerializeObject(requestVoteResponse, _jsonSerializerSettings));
            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(e, _jsonSerializerSettings));
                _logger.LogError($"THERE WAS A PROBLEM ON NODE {_node.Id}", e);
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
                    JsonConvert.DeserializeObject<AppendEntriesRequest>(json, _jsonSerializerSettings);
                _logger.LogDebug(
                    $"{context.Request.Path} called, my state is {_node.State.GetType().FullName}");

                var appendEntriesResponse = await _node.AppendEntriesAsync(appendEntries);
                await context.Response.WriteAsync(
                    JsonConvert.SerializeObject(appendEntriesResponse, _jsonSerializerSettings));
            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(e, _jsonSerializerSettings));
                _logger.LogError($"THERE WAS A PROBLEM ON NODE {_node.State}", e);
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
                        var code = _businessCodeCache.Get(command.ApplicationId, command.CodeType);
                        command = new GenerateBusinessCodeCommand(command.ApplicationId, command.CodeType, DateTime.Now);
                    }
                    var commandResponse = await _node.ExecuteCommandAsync(command);
                    await context.Response.WriteAsync(
                        JsonConvert.SerializeObject(commandResponse, _jsonSerializerSettings));
                }
                else
                {
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    await context.Response.WriteAsync("Bad command request.");
                }
            }
            catch (Exception e)
            {
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                await context.Response.WriteAsync(JsonConvert.SerializeObject(e, _jsonSerializerSettings));
                _logger.LogError($"THERE WAS A PROBLEM ON NODE {_node.Id}", e);
            }
        }
    }
}