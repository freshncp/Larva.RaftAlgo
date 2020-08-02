using System;
using Larva.RaftAlgo.Concensus.Node;

namespace Larva.RaftAlgo.Concensus.Rpc.Messages
{
    /// <summary>
    /// Node info
    /// </summary>
    public sealed class NodeInfo
    {
        /// <summary>
        /// Node info
        /// </summary>
        /// <param name="id"></param>
        /// <param name="serviceUri"></param>
        /// <param name="state"></param>
        /// <param name="role"></param>
        public NodeInfo(string id, Uri serviceUri, NodeState state, NodeRole role)
        {
            Id = id;
            ServiceUri = serviceUri;
            State = state;
            Role = role;
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
        public NodeState State { get; private set; }

        /// <summary>
        /// Node's role
        /// </summary>
        public NodeRole Role { get; private set; }
    }
}