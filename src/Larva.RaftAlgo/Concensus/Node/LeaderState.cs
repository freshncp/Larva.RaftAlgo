namespace Larva.RaftAlgo.Concensus.Node
{
    /// <summary>
    /// 
    /// </summary>
    public sealed class LeaderState : NodeState
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentTerm"></param>
        /// <param name="votedFor"></param>
        /// <param name="commitIndex"></param>
        /// <param name="lastApplied"></param>
        public LeaderState(long currentTerm, string votedFor, long commitIndex, long lastApplied)
            : base(currentTerm, votedFor, commitIndex, lastApplied)
        {
        }
    }
}