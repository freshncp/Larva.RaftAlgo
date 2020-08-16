using System;
using System.Linq;
using Larva.RaftAlgo.Concensus.Cluster;
using Larva.RaftAlgo.Concensus.Node;
using Microsoft.Extensions.Options;

namespace BusinessCodeGenerator.Raft
{
    /// <summary>
    /// Cluster settings
    /// </summary>
    public class FromConfigRaftClusterSettings : IClusterSettings
    {
        public FromConfigRaftClusterSettings(IOptions<RaftClusterConfig> options)
        {
            if (options.Value.Nodes != null)
            {
                Nodes = options.Value.Nodes.Select(s => new NodeId(s.Id, new Uri(s.ServiceUri))).ToArray();
            }
        }

        /// <summary>
        /// Nodes
        /// </summary>
        public NodeId[] Nodes { get; set; }
    }
}