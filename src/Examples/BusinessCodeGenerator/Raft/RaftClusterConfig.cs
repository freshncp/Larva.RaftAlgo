namespace BusinessCodeGenerator.Raft
{
    /// <summary>
    /// Raft cluster config
    /// </summary>
    public class RaftClusterConfig
    {
        /// <summary>
        /// Nodes
        /// </summary>
        public RaftNodeConfig[] Nodes { get; set; }
    }
}