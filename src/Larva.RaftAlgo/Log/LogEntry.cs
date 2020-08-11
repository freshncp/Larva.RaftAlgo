using System;

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
        /// <param name="commandType"></param>
        /// <param name="commandData"></param>
        /// <param name="term"></param>
        public LogEntry(string commandType, byte[] commandData, long term)
        {
            CommandType = commandType;
            CommandData = commandData;
            Term = term;
        }

        /// <summary>
        /// Command's type
        /// </summary>
        public string CommandType { get; private set; }

        /// <summary>
        /// Command's data for state machine
        /// </summary>
        public byte[] CommandData { get; private set; }

        /// <summary>
        /// Term when entry was received by leader (first index is 1)
        /// </summary>
        public long Term { get; private set; }
    }
}