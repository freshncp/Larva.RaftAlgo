using System;

namespace Larva.RaftAlgo.Concensus
{
    /// <summary>
    /// Election timeout random
    /// </summary>
    public interface IElectionTimeoutRandom
    {
        /// <summary>
        /// Get election timeout
        /// </summary>
        /// <returns></returns>
        TimeSpan GetElectionTimeout();
    }
}