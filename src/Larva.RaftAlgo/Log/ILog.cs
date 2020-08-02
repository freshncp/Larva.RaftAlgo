using System.Collections.Generic;
using System.Threading.Tasks;

namespace Larva.RaftAlgo.Log
{
    /// <summary>
    /// Log
    /// </summary>
    public interface ILog
    {
        /// <summary>
        /// Get last term and index
        /// </summary>
        /// <returns></returns>
        Task<(long term, long index)> GetLastTermAndIndexAsync();

        /// <summary>
        /// Get term by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Task<long> GetTermAtIndexAsync(long index);

        /// <summary>
        /// Get log entry by index
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        Task<LogEntry> GetAsync(long index);

        /// <summary>
        /// Get log entry list from specific index
        /// </summary>
        /// <param name="index"></param>
        /// <param name="takeCount"></param>
        /// <returns></returns>
        Task<LogEntry[]> GetListFromAsync(long index, int takeCount);

        /// <summary>
        /// Log entry count
        /// </summary>
        /// <returns></returns>
        Task<long> CountAsync();

        /// <summary>
        /// If an existing entry conflicts with a new one (same index
        /// but different terms), delete the existing entry and all that
        /// follow it (§5.3)
        /// </summary>
        /// <param name="prevLogIndex"></param>
        /// <param name="newLogEntries"></param>
        /// <returns></returns>
        Task DeleteConflictsFromThisLogAsync(long prevLogIndex, LogEntry[] newLogEntries);

        /// <summary>
        /// Batch append log entries
        /// </summary>
        /// <param name="newLogEntry"></param>
        /// <returns></returns>
        Task AppendAsync(LogEntry newLogEntry);

        /// <summary>
        /// Batch append log entries
        /// </summary>
        /// <param name="newLogEntries"></param>
        /// <returns></returns>
        Task BatchAppendAsync(LogEntry[] newLogEntries);
    }
}