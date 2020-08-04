using System;
using Larva.RaftAlgo.Concensus.Node;

namespace Larva.RaftAlgo.Concensus
{
    /// <summary>
    /// Election timeout random
    /// </summary>
    public sealed class DefaultElectionTimeoutRandom : IElectionTimeoutRandom
    {
        private readonly int _minElectionTimeoutMilliseconds;
        private readonly int _maxElectionTimeoutMilliseconds;
        private readonly Random _random;

        /// <summary>
        /// Election timeout random
        /// </summary>
        /// <param name="settings"></param>
        /// <param name="nodeId"></param>
        public DefaultElectionTimeoutRandom(IRaftSettings settings, NodeId nodeId)
        {
            _minElectionTimeoutMilliseconds = Math.Max(1000, settings.MinElectionTimeoutMilliseconds);
            _maxElectionTimeoutMilliseconds = Math.Max(_minElectionTimeoutMilliseconds + 1000, settings.MaxElectionTimeoutMilliseconds);
            _random = new Random(nodeId.Id.GetHashCode());
        }

        /// <summary>
        /// Get election timeout
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetElectionTimeout()
        {
            return TimeSpan.FromMilliseconds(_random.Next(_minElectionTimeoutMilliseconds / 500, _maxElectionTimeoutMilliseconds / 500) * 500);
        }
    }
}