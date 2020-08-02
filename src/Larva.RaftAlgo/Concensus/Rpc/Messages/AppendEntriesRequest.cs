using System.Collections.Generic;
using Larva.RaftAlgo.Log;

namespace Larva.RaftAlgo.Concensus.Rpc.Messages
{

    /// <summary>
    /// AppendEntries request
    /// Invoked by leader to replicate log entries (§5.3); also used as heartbeat (§5.2).
    /// </summary>
    public sealed class AppendEntriesRequest
    {
        /// <summary>
        /// AppendEntries request
        /// </summary>
        /// <param name="term"></param>
        /// <param name="leaderId"></param>
        /// <param name="prevLogIndex"></param>
        /// <param name="prevLogTerm"></param>
        /// <param name="entries"></param>
        /// <param name="leaderCommit"></param>
        public AppendEntriesRequest(long term, string leaderId, long prevLogIndex, long prevLogTerm, LogEntry[] entries, long leaderCommit)
        {
            Term = term;
            LeaderId = leaderId;
            PrevLogIndex = prevLogIndex;
            PrevLogTerm = prevLogTerm;
            Entries = entries;
            LeaderCommit = leaderCommit;
        }

        /// <summary>
        /// leader’s term
        /// </summary>
        public long Term { get; private set; }

        /// <summary>
        /// so follower can redirect clients
        /// </summary>
        public string LeaderId { get; private set; }

        /// <summary>
        /// index of log entry immediately preceding new ones
        /// </summary>
        public long PrevLogIndex { get; private set; }

        /// <summary>
        /// term of prevLogIndex entry
        /// </summary>
        public long PrevLogTerm { get; private set; }

        /// <summary>
        /// log entries to store (empty for heartbeat; may send more than one for efficiency)
        /// </summary>
        public LogEntry[] Entries { get; private set; }

        /// <summary>
        /// leader’s commitIndex
        /// </summary>
        public long LeaderCommit { get; private set; }
    }
}