using Larva.RaftAlgo.Concensus.Node;

namespace Larva.RaftAlgo.Concensus.Cluster
{
    /// <summary>
    /// Cluster settings
    /// </summary>
    public interface IClusterSettings
    {
        /// <summary>
        /// Nodes
        /// </summary>
        NodeId[] Nodes { get; }
    }
}