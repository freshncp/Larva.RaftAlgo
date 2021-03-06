using System;
using Larva.RaftAlgo.Log;
using Xunit;

namespace Larva.RaftAlgo.Tests
{
    public class LogTests
    {
        [Fact]
        public void AppendLogEntry()
        {
            ILog log = new InMemoryLog();
            var newLogEntry = new LogEntry("System.String", null, 123L);
            log.AppendAsync(newLogEntry).Wait();

            var lastLog = log.GetAsync(1L).Result;
            Assert.NotNull(lastLog);
            Assert.Equal(newLogEntry.CommandData, lastLog.CommandData);
            Assert.Equal(newLogEntry.Term, lastLog.Term);
        }

        [Fact]
        public void BatchAppendLogEntry()
        {
            ILog log = new InMemoryLog();
            var newLogEntries = new LogEntry[] {
                new LogEntry("System.String", null, 123L),
                new LogEntry("System.String", null, 123L),
                new LogEntry("System.String", null, 124L),
                new LogEntry("System.String", null, 124L),
                new LogEntry("System.String", null, 124L)
            };
            log.BatchAppendAsync(newLogEntries).Wait();

            for (var logIndex = 1L; logIndex <= newLogEntries.Length; logIndex++)
            {
                var lastLog = log.GetAsync(logIndex).Result;
                Assert.NotNull(lastLog);
                Assert.Equal(newLogEntries[logIndex - 1].CommandData, lastLog.CommandData);
                Assert.Equal(newLogEntries[logIndex - 1].Term, lastLog.Term);
            }
        }

        [Fact]
        public void DeleteConflictsFromThisLogThatNoConflicts()
        {
            ILog log = new InMemoryLog();
            var newLogEntries = new LogEntry[] {
                new LogEntry("System.String", null, 123L),
                new LogEntry("System.String", null, 123L),
                new LogEntry("System.String", null, 124L),
                new LogEntry("System.String", null, 124L),
                new LogEntry("System.String", null, 124L)
            };
            log.BatchAppendAsync(newLogEntries).Wait();

            var newLogEntries2 = new LogEntry[] {
                new LogEntry("System.String", null, 124L),
                new LogEntry("System.String", null, 124L)
            };
            log.DeleteConflictsFromThisLogAsync(2, newLogEntries2).Wait();
            Assert.Equal(newLogEntries.Length, log.CountAsync().Result);
        }

        [Fact]
        public void DeleteConflictsFromThisLogThatHasConflicts()
        {
            ILog log = new InMemoryLog();
            var newLogEntries = new LogEntry[] {
                new LogEntry("System.String", null, 123L),
                new LogEntry("System.String", null, 123L),
                new LogEntry("System.String", null, 124L),
                new LogEntry("System.String", null, 124L),
                new LogEntry("System.String", null, 124L)
            };
            log.BatchAppendAsync(newLogEntries).Wait();

            var newLogEntries2 = new LogEntry[] {
                new LogEntry("System.String", null, 124L),
                new LogEntry("System.String", null, 125L)
            };
            log.DeleteConflictsFromThisLogAsync(2, newLogEntries2).Wait();
            Assert.Equal(newLogEntries.Length - 2, log.CountAsync().Result);
            Assert.Null(log.GetAsync(4L).Result);// conflict at log index 4
            Assert.Null(log.GetAsync(5L).Result);
        }
    }
}
