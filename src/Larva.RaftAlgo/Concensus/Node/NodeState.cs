namespace Larva.RaftAlgo.Concensus.Node
{
    /// <summary>
    /// 
    /// </summary>
    public class NodeState
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="currentTerm"></param>
        /// <param name="votedFor"></param>
        /// <param name="commitIndex"></param>
        /// <param name="lastApplied"></param>
        public NodeState(long currentTerm, string votedFor, long commitIndex, long lastApplied)
        {
            CurrentTerm = currentTerm;
            VotedFor = votedFor;
            CommitIndex = commitIndex;
            LastApplied = lastApplied;
        }

        /// <summary>
        /// latest term server has seen (initialized to 0 on first boot, increases monotonically)
        /// </summary>
        public long CurrentTerm { get; private set; }

        /// <summary>
        /// candidateId that received vote in current term (or null if none)
        /// </summary>
        public string VotedFor { get; private set; }

        /// <summary>
        /// index of highest log entry known to be committed (initialized to 0, increases monotonically)
        /// </summary>
        public long CommitIndex { get; private set; }
        
        /// <summary>
        /// index of highest log entry applied to state machine (initialized to 0, increases monotonically)
        /// </summary>
        public long LastApplied { get; private set; }
    }
}