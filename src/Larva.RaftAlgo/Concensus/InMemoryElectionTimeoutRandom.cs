using System;

namespace Larva.RaftAlgo.Concensus
{
    /// <summary>
    /// Election timeout random
    /// </summary>
    public class InMemoryElectionTimeoutRandom : IElectionTimeoutRandom
    {
        private readonly int _minElectionTimeoutMilliseconds;
        private readonly int _maxElectionTimeoutMilliseconds;
        private readonly Random _random;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="settings"></param>
        public InMemoryElectionTimeoutRandom(IRaftSettings settings)
        {
            _minElectionTimeoutMilliseconds = settings.MinElectionTimeoutMilliseconds < 1000 ? 1000 : settings.MinElectionTimeoutMilliseconds;
            _maxElectionTimeoutMilliseconds = Math.Max(_minElectionTimeoutMilliseconds + 1000, settings.MaxElectionTimeoutMilliseconds < 2000 ? 2000 : settings.MaxElectionTimeoutMilliseconds);
            _random = new Random(DateTime.Now.Millisecond);
        }

        /// <summary>
        /// Get election timeout
        /// </summary>
        /// <returns></returns>
        public TimeSpan GetElectionTimeout()
        {
            return TimeSpan.FromMilliseconds(_random.Next(_minElectionTimeoutMilliseconds / 100, _maxElectionTimeoutMilliseconds / 100) * 100);
        }
    }
}