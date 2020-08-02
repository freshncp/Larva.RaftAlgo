using System.Net.Http;
using System.Threading.Tasks;
using Larva.RaftAlgo.Concensus.Rpc;
using Larva.RaftAlgo.Concensus.Rpc.Messages;

namespace BusinessCodeGenerator.Raft
{
    /// <summary>
    /// Http rpc client
    /// </summary>
    public class HttpRpcClient : IRpcClient
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="httpClient"></param>
        public HttpRpcClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<RequestVoteResponse> RequestVoteAsync(RequestVoteRequest request)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AppendEntriesResponse> AppendEntriesAsync(AppendEntriesRequest request)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<ExecuteCommandResponse> ExecuteCommandAsync<T>(T command)
        {
            throw new System.NotImplementedException();
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