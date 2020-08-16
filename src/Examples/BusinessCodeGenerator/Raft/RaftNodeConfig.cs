namespace BusinessCodeGenerator.Raft
{
    /// <summary>
    /// Raft node config
    /// </summary>
    public class RaftNodeConfig
    {
        /// <summary>
        /// Node's id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Node's service uri
        /// </summary>
        public string ServiceUri { get; set; }
    }
}