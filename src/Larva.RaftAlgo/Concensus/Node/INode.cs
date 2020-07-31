using System;

namespace Larva.RaftAlgo.Concensus.Node
{
    /// <summary>
    /// Node
    /// </summary>
    public interface INode
    {
        /// <summary>
        /// Node's id
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Service Uri
        /// </summary>
        Uri ServiceUri { get; }
    }
}