using Larva.RaftAlgo.Concensus.Node;
using Larva.RaftAlgo.Log;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessCodeGenerator.Raft
{
    public class SqlLiteLog : ILog
    {
        private readonly string _path;
        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim(LockRecursionPolicy.NoRecursion);
        private readonly ILogger _logger;
        private readonly string _nodeId;
        private readonly JsonSerializerSettings _jsonSerializerSettings = new JsonSerializerSettings()
        {
            TypeNameHandling = TypeNameHandling.All
        };

        public SqlLiteLog(NodeId nodeId, ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<SqlLiteLog>();
            _nodeId = nodeId.Id;
            _path = $"{_nodeId}-log.db";
            _lock.EnterWriteLock();

            if (!File.Exists(_path))
            {
                var fs = File.Create(_path);
                fs.Dispose();

                using (var connection = new SqliteConnection($"Data Source={_path};"))
                {
                    connection.Open();

                    const string sql = @"create table logs (
                        id integer primary key,
                        term integer not null,
                        command text null
                    )";
                    _logger.LogDebug($"id: {_nodeId}, sql: {sql}");
                    using (var createCommand = new SqliteCommand(sql, connection))
                    {
                        var result = createCommand.ExecuteNonQuery();
                    }
                }
            }

            _lock.ExitWriteLock();
        }

        public async Task<(long term, long index)> GetLastTermAndIndexAsync()
        {
            _lock.EnterReadLock();
            try
            {
                using (var connection = new SqliteConnection($"Data Source={_path};"))
                {
                    await connection.OpenAsync();

                    const string sql = @"select term, id from logs order by id desc limit 1";
                    _logger.LogDebug($"id: {_nodeId}, sql: {sql}");
                    using (var selectCommand = new SqliteCommand(sql, connection))
                    {
                        using (var reader = await selectCommand.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var lastTerm = reader.GetInt64(0);
                                var lastIndex = reader.GetInt64(1);
                                return (lastTerm, lastIndex);
                            }
                        }
                    }
                }
                return (0L, 0L);
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task<long> GetTermAtIndexAsync(long index)
        {
            _lock.EnterReadLock();
            try
            {
                using (var connection = new SqliteConnection($"Data Source={_path};"))
                {
                    await connection.OpenAsync();

                    var sql = $"select term from logs where id = {index}";
                    _logger.LogDebug($"id: {_nodeId}, sql: {sql}");
                    using (var selectCommand = new SqliteCommand(sql, connection))
                    {
                        var term = Convert.ToInt64(await selectCommand.ExecuteScalarAsync());
                        return term;
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task<LogEntry> GetAsync(long index)
        {
            _lock.EnterReadLock();
            try
            {
                using (var connection = new SqliteConnection($"Data Source={_path};"))
                {
                    connection.Open();

                    var sql = $"select term, command from logs where id = {index}";
                    _logger.LogDebug($"id: {_nodeId}, sql: {sql}");
                    using (var selectCommand = new SqliteCommand(sql, connection))
                    {
                        using (var reader = await selectCommand.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                var command = JsonConvert.DeserializeObject(reader.GetString(1), _jsonSerializerSettings);
                                return new LogEntry(command, reader.GetInt64(0));
                            }
                        }
                    }
                }
                return null;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task<LogEntry[]> GetListFromAsync(long index, int takeCount)
        {
            _lock.EnterReadLock();
            try
            {
                var logsToReturn = new List<LogEntry>();
                using (var connection = new SqliteConnection($"Data Source={_path};"))
                {
                    await connection.OpenAsync();
                    var sql = $"select term, command from logs where id >= {index}";
                    _logger.LogDebug($"id: {_nodeId}, sql: {sql}");
                    using (var selectCommand = new SqliteCommand(sql, connection))
                    {
                        using (var reader = await selectCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                var command = JsonConvert.DeserializeObject(reader.GetString(1), _jsonSerializerSettings);
                                var logEntry = new LogEntry(command, reader.GetInt64(0));
                                logsToReturn.Add(logEntry);
                            }
                        }
                    }
                    return logsToReturn.ToArray();
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task<long> CountAsync()
        {
            _lock.EnterReadLock();
            try
            {
                using (var connection = new SqliteConnection($"Data Source={_path};"))
                {
                    connection.Open();
                    const string sql = @"select count(id) from logs";
                    _logger.LogDebug($"id: {_nodeId}, sql: {sql}");
                    using (var selectCommand = new SqliteCommand(sql, connection))
                    {
                        var index = Convert.ToInt64(await selectCommand.ExecuteScalarAsync());
                        return index < 0 ? 0L : index;
                    }
                }
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public async Task DeleteConflictsFromThisLogAsync(long prevLogIndex, LogEntry[] newLogEntries)
        {
            if (newLogEntries == null || newLogEntries.Length == 0) return;

            _lock.EnterWriteLock();
            try
            {
                using (var connection = new SqliteConnection($"Data Source={_path};"))
                {
                    connection.Open();
                    var logTerms = new List<long>();
                    var sql = $"select term from logs where id > {prevLogIndex} order by id limit {newLogEntries.Length};";
                    _logger.LogDebug($"id: {_nodeId} sql: {sql}");
                    using (var selectCommand = new SqliteCommand(sql, connection))
                    {
                        using (var reader = await selectCommand.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                logTerms.Add(reader.GetInt64(0));
                            }
                        }
                    }
                    for (var i = 0; i < newLogEntries.Length; i++)
                    {
                        var newLogIndex = prevLogIndex + i + 1;
                        var newLogTerm = newLogEntries[i].Term;

                        if (logTerms.Count > i && logTerms[i] != newLogTerm)
                        {
                            var deleteSql = $"delete from logs where id >= {newLogIndex};";
                            _logger.LogDebug($"id: {_nodeId}, deleteSql: {deleteSql}");
                            using (var deleteCommand = new SqliteCommand(deleteSql, connection))
                            {
                                var result = await deleteCommand.ExecuteNonQueryAsync();
                            }
                        }
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public async Task AppendAsync(LogEntry newLogEntry)
        {
            _lock.EnterWriteLock();
            try
            {
                using (var connection = new SqliteConnection($"Data Source={_path};"))
                {
                    await connection.OpenAsync();
                    var commandData = JsonConvert.SerializeObject(newLogEntry.Command, _jsonSerializerSettings);
                    var sql = $"insert into logs (term, command) values ({newLogEntry.Term}, '{commandData.Replace("'", "''")}')";
                    _logger.LogDebug($"id: {_nodeId}, sql: {sql}");
                    using (var insertCommand = new SqliteCommand(sql, connection))
                    {
                        var result = await insertCommand.ExecuteNonQueryAsync();
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public async Task BatchAppendAsync(LogEntry[] newLogEntries)
        {
            if (newLogEntries == null || newLogEntries.Length == 0)
            {
                return;
            }

            if (newLogEntries.Length == 1)
            {
                await AppendAsync(newLogEntries[0]);
            }

            _lock.EnterWriteLock();
            try
            {
                using (var connection = new SqliteConnection($"Data Source={_path};"))
                {
                    connection.Open();
                    var firstCommandData = JsonConvert.SerializeObject(newLogEntries[0].Command, _jsonSerializerSettings);
                    var sqlBuilder = new StringBuilder($@"insert into logs (term, command)
select {newLogEntries[0].Term} as term, '{firstCommandData.Replace("'", "''")}' as command
");
                    for (var i = 1; i < newLogEntries.Length; i++)
                    {
                        var newLogEntry = newLogEntries[i];
                        var commandData = JsonConvert.SerializeObject(newLogEntries[i].Command, _jsonSerializerSettings);
                        sqlBuilder.AppendLine($"union select {newLogEntry.Term}, '{commandData.Replace("'", "''")}' ");
                    }
                    _logger.LogDebug($"id: {_nodeId}, sql: {sqlBuilder}");
                    Console.WriteLine($"id: {_nodeId}, sql: {sqlBuilder}");
                    using (var insertCommand = new SqliteCommand(sqlBuilder.ToString(), connection))
                    {
                        var result = await insertCommand.ExecuteNonQueryAsync();
                    }
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }
}
