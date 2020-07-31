using System.Threading.Tasks;
using Larva.RaftAlgo.Log;

namespace Larva.RaftAlgo.StateMachine
{
    /// <summary>
    /// Replicated state machine
    /// </summary>
    public interface IReplicatedStateMachine
    {
        /// <summary>
        /// Handle logentry
        /// </summary>
        /// <param name="log"></param>
        /// <returns></returns>
        Task<object> HandleAsync(LogEntry log);
    }
}