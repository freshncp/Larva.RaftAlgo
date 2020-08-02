using System;
using System.Threading.Tasks;
using Larva.RaftAlgo.Concensus.Rpc;
using Larva.RaftAlgo.Concensus.Rpc.Messages;

namespace Larva.RaftAlgo.Concensus.Node
{
    /// <summary>
    /// Remote node
    /// </summary>
    public sealed class RemoteNode : INode
    {
        private readonly IRpcClient _rpcClient;

        /// <summary>
        /// Remote node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="serviceUri"></param>
        /// <param name="rpcClient"></param>
        public RemoteNode(string id, Uri serviceUri, IRpcClient rpcClient)
        {
            Id = id;
            ServiceUri = serviceUri;
            _rpcClient = rpcClient;
        }

        /// <summary>
        /// Node's id
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Service Uri
        /// </summary>
        public Uri ServiceUri { get; private set; }

        /// <summary>
        /// Node's state
        /// </summary>
        public NodeState State
        {
            get
            {
                var result = _rpcClient.QueryNodeInfoAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                return result?.Node?.State;
            }
        }

        /// <summary>
        /// Node's role
        /// </summary>
        public NodeRole Role
        {
            get
            {
                var result = _rpcClient.QueryNodeInfoAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                return result?.Node?.Role ?? NodeRole.Follower;
            }
        }

        /// <summary>
        /// Is remote node
        /// </summary>
        public bool IsRemote => true;

        /// <summary>
        /// Join specific node to remote node.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="serviceUri"></param>
        /// <returns></returns>
        public async Task JoinAsync(string id, Uri serviceUri)
        {
            await _rpcClient.AddNodeToClusterAsync(new Rpc.Messages.AddNodeToClusterRequest(id, serviceUri));
        }

        #region RPC Request

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<RequestVoteResponse> RequestVoteAsync(RequestVoteRequest request)
        {
            return await _rpcClient.RequestVoteAsync(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AppendEntriesResponse> AppendEntriesAsync(AppendEntriesRequest request)
        {
            return await _rpcClient.AppendEntriesAsync(request);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="command"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<ExecuteCommandResponse> ExecuteCommandAsync<T>(T command)
        {
            return await _rpcClient.ExecuteCommandAsync(command);
        }

        /// <summary>
        /// Query node info
        /// </summary>
        /// <returns></returns>
        public async Task<QueryNodeInfoResponse> QueryNodeInfoAsync()
        {
            return await _rpcClient.QueryNodeInfoAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<QueryClusterInfoResponse> QueryClusterInfoAsync()
        {
            return await _rpcClient.QueryClusterInfoAsync();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<AddNodeToClusterResponse> AddNodeToClusterAsync(AddNodeToClusterRequest request)
        {
            return await _rpcClient.AddNodeToClusterAsync(request);
        }

        #endregion
    }
}