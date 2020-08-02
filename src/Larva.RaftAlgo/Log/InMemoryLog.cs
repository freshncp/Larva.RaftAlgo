using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Larva.RaftAlgo.Log
{
    /// <summary>
    /// 
    /// </summary>
    public class InMemoryLog : ILog
    {
        private readonly Dictionary<long, LogEntry> _log = new Dictionary<long, LogEntry>();
        private readonly ReaderWriterLockSlim _logLock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<(long term, long index)> GetLastTermAndIndexAsync()
        {
            _logLock.EnterReadLock();
            try
            {
                if (_log.Count == 0)
                {
                    return Task.FromResult((0L, 0L));
                }
                var lastIndex = _log.Count == 0 ? 0L : _log.Keys.Max();
                return Task.FromResult((_log[lastIndex].Term, lastIndex));
            }
            finally
            {
                _logLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Task<LogEntry> GetAsync(long index)
        {
            _logLock.EnterReadLock();
            try
            {
                _log.TryGetValue(index, out LogEntry logEntry);
                return Task.FromResult(logEntry);
            }
            finally
            {
                _logLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <param name="takeCount"></param>
        /// <returns></returns>
        public Task<LogEntry[]> GetListFromAsync(long index, int takeCount)
        {
            _logLock.EnterReadLock();
            try
            {
                return Task.FromResult(_log.Where(w => w.Key >= index).OrderBy(o => o.Key).Select(s => s.Value).ToArray());
            }
            finally
            {
                _logLock.EnterReadLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public Task<long> GetTermAtIndexAsync(long index)
        {
            _logLock.EnterReadLock();
            try
            {
                _log.TryGetValue(index, out LogEntry logEntry);
                return Task.FromResult(logEntry?.Term ?? 0L);
            }
            finally
            {
                _logLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public Task<long> CountAsync()
        {
            _logLock.EnterReadLock();
            try
            {
                return Task.FromResult((long)_log.Count);
            }
            finally
            {
                _logLock.ExitReadLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="prevLogIndex"></param>
        /// <param name="newLogEntries"></param>
        /// <returns></returns>
        public Task DeleteConflictsFromThisLogAsync(long prevLogIndex, LogEntry[] newLogEntries)
        {
            _logLock.EnterWriteLock();
            try
            {
                for (var i = 0; i < newLogEntries.Length; i++)
                {
                    var newLogIndex = prevLogIndex + 1 + i;
                    var newLogEntry = newLogEntries[i];
                    if (_log.TryGetValue(newLogIndex, out LogEntry logEntry) && logEntry.Term != newLogEntry.Term)
                    {
                        var lastIndex = _log.Count == 0 ? 0L : _log.Keys.Max();
                        for (var j = newLogIndex; j <= lastIndex; j++)
                        {
                            _log.Remove(j);
                        }
                    }
                }
                return Task.CompletedTask;
            }
            finally
            {
                _logLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newLogEntry"></param>
        /// <returns></returns>
        public Task AppendAsync(LogEntry newLogEntry)
        {
            _logLock.EnterWriteLock();
            try
            {
                var lastIndex = _log.Count == 0 ? 0L : _log.Keys.Max();
                _log.Add(lastIndex + 1, newLogEntry);
                return Task.CompletedTask;
            }
            finally
            {
                _logLock.ExitWriteLock();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="newLogEntries"></param>
        /// <returns></returns>
        public Task BatchAppendAsync(LogEntry[] newLogEntries)
        {
            _logLock.EnterWriteLock();
            try
            {
                var lastIndex = _log.Count == 0 ? 0L : _log.Keys.Max();
                for (var i = 0; i < newLogEntries.Length; i++)
                {
                    _log.Add(lastIndex + i + 1, newLogEntries[i]);
                }
                return Task.CompletedTask;
            }
            finally
            {
                _logLock.ExitWriteLock();
            }
        }
    }
}