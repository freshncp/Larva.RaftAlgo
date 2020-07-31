using System.Threading.Tasks;
using Larva.RaftAlgo.Concensus.Node;

namespace Larva.RaftAlgo.Concensus
{
    /// <summary>
    /// 
    /// </summary>
    public interface ICluster
    {
        /// <summary>
        /// 
        /// </summary>
        LocalNode CurrentNode { get; }

        /// <summary>
        /// 
        /// </summary>
        RemoteNode[] Members { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        Task Join(RemoteNode node);
    }
}