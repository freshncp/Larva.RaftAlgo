using System;

namespace Larva.RaftAlgo.Concensus.Rpc.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class AddNodeToClusterRequest
    {
        /// <summary>
        /// Remote node
        /// </summary>
        /// <param name="id"></param>
        /// <param name="serviceUri"></param>
        public AddNodeToClusterRequest(string id, Uri serviceUri)
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