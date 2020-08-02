namespace Larva.RaftAlgo
{
    /// <summary>
    /// Raft settings
    /// </summary>
    public class InMemoryRaftSettings : IRaftSettings
    {
        /// <summary>
        /// 
        /// </summary>
        public int MinElectionTimeoutMilliseconds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int MaxElectionTimeoutMilliseconds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int HeartbeatMilliseconds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int CommandTimeoutMilliseconds { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public int AppendEntriesBatchSize { get; set; }
    }
}