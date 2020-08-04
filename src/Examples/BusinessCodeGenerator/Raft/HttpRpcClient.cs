using System;
using System.Net.Http;
using System.Threading.Tasks;
using Larva.RaftAlgo.Concensus.Rpc;
using Larva.RaftAlgo.Concensus.Rpc.Messages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace BusinessCodeGenerator.Raft
{
    /// <summary>
    /// Http rpc client
    /// </summary>
    public class HttpRpcClient : IRpcClient
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly JsonSerializerSettings _jsonSerializerSettings;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        public HttpRpcClient(HttpClient httpClient, ILoggerFactory loggerFactory)
        {
            _httpClient = httpClient;
            _logger = loggerFactory.CreateLogger("HttpRpcClient");
            _jsonSerializerSettings = new JsonSerializerSettings()
            {
                TypeNameHandling = TypeNameHandling.All
            };
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RequestVoteResponse> RequestVoteAsync(RequestVoteRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var response = await _httpClient.PostAsync("/_raft/requestvote", content);
                if (response != null && response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<RequestVoteResponse>(responseContent);
                }
                return new RequestVoteResponse(request.Term, false, "Server Internal Error");
            }
            catch (HttpRequestException requestEx)
            {
                _logger.LogError($"RequestVote fail: {requestEx.Message}, serverUri: {_httpClient.BaseAddress}");
                return new RequestVoteResponse(request.Term, false, requestEx.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"RequestVote fail: {ex.Message}, serverUri: {_httpClient.BaseAddress}");
                return new RequestVoteResponse(request.Term, false, ex.Message);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AppendEntriesResponse> AppendEntriesAsync(AppendEntriesRequest request)
        {
            try
            {
                var json = JsonConvert.SerializeObject(request);
                var content = new StringContent(json);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var response = await _httpClient.PostAsync("/_raft/appendEntries", content);
                if (response != null && response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<AppendEntriesResponse>(responseContent);
                }
                return new AppendEntriesResponse(request.Term, false);
            }
            catch (HttpRequestException requestEx)
            {
                _logger.LogError($"AppendEntries fail: {requestEx.Message}, serverUri: {_httpClient.BaseAddress}");
                return new AppendEntriesResponse(request.Term, false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"AppendEntries fail: {ex.Message}, serverUri: {_httpClient.BaseAddress}");
                return new AppendEntriesResponse(request.Term, false);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<ExecuteCommandResponse> ExecuteCommandAsync<T>(T command)
        {
            Console.WriteLine("SENDING REQUEST....");
            try
            {
                var json = JsonConvert.SerializeObject(command, _jsonSerializerSettings);
                var content = new StringContent(json);
                content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/json");
                var response = await _httpClient.PostAsync("/_raft/command", content);
                if (response != null && response.IsSuccessStatusCode)
                {
                    Console.WriteLine("REQUEST OK....");
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<ExecuteCommandResponse>(responseContent, _jsonSerializerSettings);
                }

                Console.WriteLine("REQUEST NOT OK....");
                return new ExecuteCommandResponse(null, false, $"Server {_httpClient.BaseAddress} request fail.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ExecuteCommand fail: {ex.Message}, serverUri: {_httpClient.BaseAddress}");
                return new ExecuteCommandResponse(null, false, $"Server {_httpClient.BaseAddress} request fail: {ex.Message}", ex.StackTrace);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AddNodeToClusterResponse> AddNodeToClusterAsync(AddNodeToClusterRequest request)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<QueryNodeInfoResponse> QueryNodeInfoAsync()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<QueryClusterInfoResponse> QueryClusterInfoAsync()
        {
            throw new System.NotImplementedException();
        }
    }
}