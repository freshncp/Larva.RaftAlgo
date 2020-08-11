using System.Threading.Tasks;
using Larva.RaftAlgo.Log;

namespace Larva.RaftAlgo.StateMachine
{
    /// <summary>
    /// In memoery replicated state machine
    /// </summary>
    public class InMemoryReplicatedStateMachine : IReplicatedStateMachine
    {
        /// <summary>
        /// Handle logentry
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        public Task<string> HandleAsync(LogEntry log)
        {
            return Task.FromResult(log.CommandData.GetType().FullName);
        }
    }
}