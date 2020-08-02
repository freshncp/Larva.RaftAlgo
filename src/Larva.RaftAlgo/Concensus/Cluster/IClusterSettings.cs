using Larva.RaftAlgo.Concensus.Node;

namespace Larva.RaftAlgo.Concensus.Cluster
{
    /// <summary>
    /// Cluster settings
    /// </summary>
    public interface IClusterSettings
    {
        /// <summary>
        /// Remote nodes
        /// </summary>
        NodeId[] RemoteNodes { get; }
    }
}