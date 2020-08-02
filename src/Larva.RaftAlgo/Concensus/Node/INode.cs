using System;
using Larva.RaftAlgo.Concensus.Rpc;

namespace Larva.RaftAlgo.Concensus.Node
{
    /// <summary>
    /// Raft Node
    /// </summary>
    public interface INode: IRpcClient
    {
        /// <summary>
        /// Node's id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Service Uri
        /// </summary>
        Uri ServiceUri { get; }

        /// <summary>
        /// Node's state
        /// </summary>
        NodeState State { get; }

        /// <summary>
        /// Node's role
        /// </summary>
        NodeRole Role { get; }
    }
}