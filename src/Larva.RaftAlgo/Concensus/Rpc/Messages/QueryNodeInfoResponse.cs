namespace Larva.RaftAlgo.Concensus.Rpc.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class QueryNodeInfoResponse
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="node"></param>
        public QueryNodeInfoResponse(NodeInfo node)
        {
            Node = node;
        }
        
        /// <summary>
        /// 
        /// </summary>
        public NodeInfo Node { get; private set; }
    }
}