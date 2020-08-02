namespace Larva.RaftAlgo.Log
{
    /// <summary>
    /// Log entry
    /// </summary>
    public class LogEntry
    {
        /// <summary>
        /// Log entry
        /// </summary>
        /// <param name="command"></param>
        /// <param name="term"></param>
        public LogEntry(string command, long term)
        {
            Command = command;
            Term = term;
        }

        /// <summary>
        /// Command for state machine
        /// </summary>
        public string Command { get; private set; }

        /// <summary>
        /// Term when entry was received by leader (first index is 1)
        /// </summary>
        public long Term { get; private set; }
    }
}