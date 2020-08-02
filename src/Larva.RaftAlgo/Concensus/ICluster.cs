using Larva.RaftAlgo.Concensus.Node;

namespace Larva.RaftAlgo.Concensus
{
    /// <summary>
    /// Raft cluster
    /// </summary>
    public interface ICluster
    {
        /// <summary>
        /// Current node
        /// </summary>
        LocalNode CurrentNode { get; }

        /// <summary>
        /// Other nodes
        /// </summary>
        RemoteNode[] OtherNodes { get; }

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