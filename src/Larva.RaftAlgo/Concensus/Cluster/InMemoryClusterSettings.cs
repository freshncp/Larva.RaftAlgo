using Larva.RaftAlgo.Concensus.Node;

namespace Larva.RaftAlgo.Concensus.Cluster
{
    /// <summary>
    /// Cluster settings
    /// </summary>
    public class InMemoryClusterSettings : IClusterSettings
    {
        /// <summary>
        /// Nodes
        /// </summary>
        public NodeId[] Nodes { get; set; }
    }
}