using System;
using System.Collections.Generic;
using Larva.RaftAlgo.Concensus.Node;
using Larva.RaftAlgo.Concensus.Rpc;

namespace Larva.RaftAlgo.Concensus.Cluster
{
    /// <summary>
    /// Raft cluster
    /// </summary>
    public class DefaultCluster : ICluster
    {
        private readonly IRpcClientProvider _rpcClientProvider;
        private readonly IClusterSettings _clusterSettings;
        private readonly List<INode> _nodes = new List<INode>();

        /// <summary>
        /// In memory raft cluster
        /// </summary>
        /// <param name="localNode"></param>
        /// <param name="rpcClientProvider"></param>
        /// <param name="clusterSettings"></param>
        public DefaultCluster(INode localNode, IRpcClientProvider rpcClientProvider, IClusterSettings clusterSettings)
        {
            if (localNode.IsRemote)
            {
                throw new ArgumentException("Must local node", nameof(localNode));
            }
            _nodes.Add(localNode);
            _rpcClientProvider = rpcClientProvider;
            _clusterSettings = clusterSettings;
        }

        /// <summary>
        /// Nodes
        /// </summary>
        public INode[] Nodes => _nodes.ToArray();

        /// <summary>
        /// Load cluster info
        /// </summary>
        public void Load()
        {
            if (_clusterSettings.Nodes != null)
            {
                foreach (var nodeId in _clusterSettings.Nodes)
                {
                    AddRemoteNode(nodeId.Id, nodeId.ServiceUri);
                }
            }
        }

        /// <summary>
        /// Is single node cluster
        /// </summary>
        /// <returns></returns>
        public bool IsSingleNodeCluster()
        {
            return _nodes.Count == 1;
        }

        /// <summary>
        /// Get node count
        /// </summary>
        /// <returns></returns>
        public int GetNodeCount()
        {
            return _nodes.Count;
        }

        /// <summary>
        /// Add remote node
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="serviceUri"></param>
        public void AddRemoteNode(string nodeId, Uri serviceUri)
        {
            if (!_nodes.Exists(e => e.Id == nodeId || e.ServiceUri == serviceUri))
            {
                var rpcClient = _rpcClientProvider.GetRpcClient(serviceUri);
                var remoteNode = new RemoteNode(nodeId, serviceUri, rpcClient);
                _nodes.Add(remoteNode);
                Console.WriteLine($"Add remote node: nodeId={nodeId}, serviceUri={serviceUri}");
            }
        }
    }
}