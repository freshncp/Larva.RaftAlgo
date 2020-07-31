namespace Larva.RaftAlgo.Concensus.Node
{
    /// <summary>
    /// 角色类型
    /// </summary>
    public enum NodeRole
    {
        /// <summary>
        /// Follower
        /// </summary>
        Follower = 0,

        /// <summary>
        /// Candidate
        /// </summary>
        Candidate = 1,

        /// <summary>
        /// Leader
        /// </summary>
        Leader = 2
    }
}