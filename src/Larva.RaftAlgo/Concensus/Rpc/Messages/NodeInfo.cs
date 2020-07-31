using System;

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
        public NodeInfo(string id, Uri serviceUri)
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