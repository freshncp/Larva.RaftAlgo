using System.Collections.Generic;
using Larva.RaftAlgo.Concensus.Node;

namespace Larva.RaftAlgo.Concensus
{
    /// <summary>
    /// In memory raft cluster
    /// </summary>
    public class InMemoryCluster : ICluster
    {
        private readonly List<RemoteNode> _otherNodes = new List<RemoteNode>();

        /// <summary>
        /// In memory raft cluster
        /// </summary>
        /// <param name="node"></param>
        public InMemoryCluster(LocalNode node)
        {
            CurrentNode = node;
        }

        /// <summary>
        /// Current node
        /// </summary>
        public LocalNode CurrentNode { get; }

        /// <summary>
        /// Other nodes
        /// </summary>
        public RemoteNode[] OtherNodes => _otherNodes.ToArray();

        /// <summary>
        /// Is single node cluster
        /// </summary>
        /// <returns></returns>
        public bool IsSingleNodeCluster()
        {
            return _otherNodes.Count == 0;
        }

        /// <summary>
        /// Get node count
        /// </summary>
        /// <returns></returns>
        public int GetNodeCount()
        {
            return _otherNodes.Count + 1;
        }

        /// <summary>
        /// Add remote node
        /// </summary>
        /// <param name="node"></param>
        public void AddRemoteNode(RemoteNode node)
        {
            if (CurrentNode.Id != node.Id && CurrentNode.ServiceUri != node.ServiceUri
               && !_otherNodes.Exists(e => e.Id == node.Id || e.ServiceUri == node.ServiceUri))
            {
                _otherNodes.Add(node);
            }
        }
    }
}