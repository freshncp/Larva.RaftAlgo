using System;
using Larva.RaftAlgo.Concensus.Node;

namespace Larva.RaftAlgo.Concensus.Node
{
    /// <summary>
    /// Node identity
    /// </summary>
    public sealed class NodeId
    {
        /// <summary>
        /// Node identity
        /// </summary>
        /// <param name="id"></param>
        /// <param name="serviceUri"></param>
        public NodeId(string id, Uri serviceUri)
        {
            Id = id;
            ServiceUri = serviceUri;
        }

        /// <summary>
        /// Node's id
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Service Uri
        /// </summary>
        public Uri ServiceUri { get; private set; }
    }
}