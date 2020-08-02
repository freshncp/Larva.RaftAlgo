using Larva.RaftAlgo.Concensus.Node;

namespace Larva.RaftAlgo.Concensus.Cluster
{
    /// <summary>
    /// Cluster settings
    /// </summary>
    public class InMemoryClusterSettings : IClusterSettings
    {
        /// <summary>
        /// Remote nodes
        /// </summary>
        public NodeId[] RemoteNodes { get; set; }
    }
}