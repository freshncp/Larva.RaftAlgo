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
        Task<RequestVoteResponse> RequestVote(RequestVoteRequest request);

        /// <summary>
        /// Make a AppendEntries RPC request to the given peer
        /// </summary>
        Task<AppendEntriesResponse> AppendEntries(AppendEntriesRequest request);

        /// <summary>
        /// Execute command.
        /// </summary>
        /// <param name="command"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        Task<ExecuteCommandResponse> ExecuteCommand<T>(T command);

        /// <summary>
        /// Query cluster info
        /// </summary>
        /// <returns></returns>
        Task<QueryClusterInfoResponse> QueryClusterInfo();

        /// <summary>
        /// Add node to cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<AddNodeToClusterResponse> AddNodeToCluster(AddNodeToClusterRequest request);
    }
}