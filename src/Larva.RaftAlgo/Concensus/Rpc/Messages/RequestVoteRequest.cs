namespace Larva.RaftAlgo.Concensus.Rpc.Messages
{
    /// <summary>
    /// RequestVote request
    /// </summary>
    public sealed class RequestVoteRequest
    {
        /// <summary>
        /// RequestVote request
        /// </summary>
        public RequestVoteRequest(long term, string candidateId, long lastLogIndex, long lastLogTerm)
        {
            Term = term;
            CandidateId = candidateId;
            LastLogIndex = lastLogIndex;
            LastLogTerm = lastLogTerm;
        }

        /// <summary>
        /// candidate’s term
        /// </summary>
        public long Term { get; private set; }

        /// <summary>
        /// candidate requesting vote
        /// </summary>
        public string CandidateId { get; private set; }

        /// <summary>
        /// index of candidate’s last log entry (§5.4)
        /// </summary>
        public long LastLogIndex { get; private set; }

        /// <summary>
        /// term of candidate’s last log entry (§5.4)
        /// </summary>
        public long LastLogTerm { get; private set; }
    }
}