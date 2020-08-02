using Larva.RaftAlgo.Concensus.Node;

namespace Larva.RaftAlgo.Concensus.Cluster
{
    /// <summary>
    /// Raft cluster
    /// </summary>
    public interface ICluster
    {
        /// <summary>
        /// Nodes
        /// </summary>
        INode[] Nodes { get; }

        /// <summary>
        /// Load cluster info
        /// </summary>
        void Load();

        /// <summary>
        /// Is single node cluster
        /// </summary>
        /// <returns></returns>
        bool IsSingleNodeCluster();

        /// <summary>
        /// Get node count
        /// </summary>
        /// <returns></returns>
        int GetNodeCount();
    }
}