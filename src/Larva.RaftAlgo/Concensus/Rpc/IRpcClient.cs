using System.Threading.Tasks;
using Larva.RaftAlgo.Concensus.Rpc.Messages;

namespace Larva.RaftAlgo.Concensus.Rpc
{
    /// <summary>
    /// Rpc client
    /// </summary>
    public interface IRpcClient
    {
        /// <summary>
        /// Make a RequestVote RPC request to the given peer
        /// </summary>
        Task<RequestVoteResponse> RequestVoteAsync(RequestVoteRequest request);

        /// <summary>
        /// Make a AppendEntries RPC request to the given peer
        /// </summary>
        Task<AppendEntriesResponse> AppendEntriesAsync(AppendEntriesRequest request);

        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="command"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<ExecuteCommandResponse> ExecuteCommandAsync<T>(T command);

        /// <summary>
        /// Query node info
        /// </summary>
        /// <returns></returns>
        Task<QueryNodeInfoResponse> QueryNodeInfoAsync();

        /// <summary>
        /// Query cluster info
        /// </summary>
        /// <returns></returns>
        Task<QueryClusterInfoResponse> QueryClusterInfoAsync();

        /// <summary>
        /// Add node to cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AddNodeToClusterResponse> AddNodeToClusterAsync(AddNodeToClusterRequest request);
    }
}