using System;
using Larva.RaftAlgo.Concensus.Rpc;

namespace Larva.RaftAlgo.Concensus.Node
{
    /// <summary>
    /// Remote node
    /// </summary>
    public sealed class RemoteNode : INode
    {
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
            RpcClient = rpcClient;
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
        /// Rpc client
        /// </summary>
        public IRpcClient RpcClient { get; private set; }
    }
}