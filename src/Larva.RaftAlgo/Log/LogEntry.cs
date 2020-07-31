namespace Larva.RaftAlgo.Log
{
    /// <summary>
    /// Log Entry
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Log Entry
        /// </summary>
        /// <param name="index"></param>
        /// <param name="command"></param>
        /// <param name="term"></param>
        public LogEntry(long index, object command, long term)
        {
            Index = index;
            Command = command;
            Term = term;
        }

        /// <summary>
        /// Log index
        /// </summary>
        /// <value></value>
        public long Index { get; }

        /// <summary>
        /// Command for state machine
        /// </summary>
        public object Command { get; }

        /// <summary>
        /// Term when entry was received by leader (first index is 1)
        /// </summary>
        public long Term { get; }
    }
}