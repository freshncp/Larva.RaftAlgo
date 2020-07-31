using System.Threading.Tasks;

namespace Larva.RaftAlgo.Log
{
    /// <summary>
    /// Log repository
    /// </summary>
    public interface ILogRepository
    {
        /// <summary>
        /// Get last term and index
        /// </summary>
        /// <returns></returns>
        Task<(long term, long index)> GetLastTermAndIndexAsync();

        /// <summary>
        /// Get logentry by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Task<LogEntry> GetLogEntryAsync(long index);
    }
}