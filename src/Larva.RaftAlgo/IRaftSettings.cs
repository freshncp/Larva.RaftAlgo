namespace Larva.RaftAlgo
{
    /// <summary>
    /// Raft settings
    /// </summary>
    public interface IRaftSettings
    {
        /// <summary>
        /// Min election timeout
        /// </summary>
        int MinElectionTimeoutMilliseconds { get; }

        /// <summary>
        /// Max election timeout
        /// </summary>
        int MaxElectionTimeoutMilliseconds { get; }

        /// <summary>
        /// Heartbeat
        /// </summary>
        int HeartbeatMilliseconds { get; }

        /// <summary>
        /// Command timeout
        /// </summary>
        int CommandTimeoutMilliseconds { get; }

        /// <summary>
        /// Append entries batch size
        /// </summary>
        int AppendEntriesBatchSize { get; }
    }
}