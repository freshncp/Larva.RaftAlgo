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
        /// <param name="reason"></param>
        public RequestVoteResponse(long term, bool voteGranted, string reason = "")
        {
            Term = term;
            VoteGranted = voteGranted;
            Reason = reason;
        }

        /// <summary>
        /// currentTerm, for candidate to update itself
        /// </summary>
        public long Term { get; private set; }

        /// <summary>
        /// true means candidate received vote
        /// </summary>
        public bool VoteGranted { get; private set; }

        /// <summary>
        /// Not grant vote reason
        /// </summary>
        /// <value></value>
        public string Reason { get; private set; }
    }
}