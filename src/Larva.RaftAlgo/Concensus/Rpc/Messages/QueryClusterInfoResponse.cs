namespace Larva.RaftAlgo.Concensus.Rpc.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryClusterInfoResponse
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="nodes"></param>
        public QueryClusterInfoResponse(NodeInfo[] nodes)
        {
            Nodes = nodes;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public NodeInfo[] Nodes { get; private set; }
    }
}