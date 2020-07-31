namespace Larva.RaftAlgo.Concensus.Rpc.Messages
{
    /// <summary>
    /// RequestVote response
    /// </summary>
    public sealed class RequestVoteResponse
    {
        /// <summary>
        /// RequestVote response
        /// </summary>
        /// <param name="term"></param>
        /// <param name="voteGranted"></param>
        public RequestVoteResponse(long term, bool voteGranted)
        {
            Term = term;
            VoteGranted = voteGranted;
        }

        /// <summary>
        /// currentTerm, for candidate to update itself
        /// </summary>
        public long Term { get; private set; }

        /// <summary>
        /// true means candidate received vote
        /// </summary>
        public bool VoteGranted { get; private set; }
    }
}