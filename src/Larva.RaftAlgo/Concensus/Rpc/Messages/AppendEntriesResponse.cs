namespace Larva.RaftAlgo.Concensus.Rpc.Messages
{
    /// <summary>
    /// AppendEntries response
    /// </summary>
    public sealed class AppendEntriesResponse
    {
        /// <summary>
        /// AppendEntries response
        /// </summary>
        /// <param name="term"></param>
        /// <param name="success"></param>
        public AppendEntriesResponse(long term, bool success)
        {
            Term = term;
            Success = success;
        }

        /// <summary>
        /// currentTerm, for leader to update itself
        /// </summary>
        public long Term { get; private set; }

        /// <summary>
        /// true if follower contained entry matching prevLogIndex and prevLogTerm
        /// </summary>
        public bool Success { get; private set; }
    }
}