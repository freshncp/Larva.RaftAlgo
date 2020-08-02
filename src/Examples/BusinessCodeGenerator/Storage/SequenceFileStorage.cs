using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BusinessCodeGenerator.Storage
{
    public class SequenceFileStorage : ISequenceBackendStorage, IDisposable
    {
        private readonly SemaphoreSlim _sempaphore = new SemaphoreSlim(1, 1);
        private readonly ILogger _logger;
        private readonly string _dataFileName = "data.bin";
        private readonly string _indexFileName = "data.idx";
        private readonly ConcurrentQueue<BusinessCodeIndex> _indexQueue = new ConcurrentQueue<BusinessCodeIndex>();

        private FileStream _dataFS = null;
        private FileStream _indexFS = null;

        public SequenceFileStorage(ILoggerFactory loggerFactory, string nodeId)
        {
            _logger = loggerFactory.CreateLogger<SequenceFileStorage>();
            _dataFileName = $"{nodeId}-data.bin";
            _indexFileName = $"{nodeId}-data.idx";

            _sempaphore.Wait();

            try
            {
                if (_dataFS == null || _indexFS == null)
                {
                    _dataFS = File.Open(_dataFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
                    _indexFS = File.Open(_indexFileName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);

                    byte[] indexBytes = new byte[BusinessCodeIndex.BYTES_LENGTH];
                    while (_indexFS.Read(indexBytes, 0, indexBytes.Length) > 0)
                    {
                        var code = new BusinessCodeIndex(indexBytes);
                        _indexQueue.Enqueue(code);
                    }
                }
            }
            finally
            {
                _sempaphore.Release();
            }
        }

        public async Task<long> GetGlobalVersion()
        {
            await _sempaphore.WaitAsync();
            try
            {
                if (_indexQueue.Count == 0)
                {
                    _dataFS.Seek(0, SeekOrigin.Begin);
                    await _dataFS.WriteAsync(BitConverter.GetBytes(0L), 0, 8);
                    await _dataFS.FlushAsync();
                    return 0L;
                }

                var globalVerBytes = new byte[8];
                _dataFS.Seek(0, SeekOrigin.Begin);
                await _dataFS.ReadAsync(globalVerBytes, 0, 8);
                return BitConverter.ToInt64(globalVerBytes);
            }
            catch
            {
                return 0L;
            }
            finally
            {
                _sempaphore.Release();
            }
        }

        public void Dispose()
        {
            if (_dataFS != null)
            {
                _dataFS.Flush();
                _dataFS.Close();
                _dataFS.Dispose();
                _dataFS = null;
            }
            if (_indexFS != null)
            {
                _indexFS.Flush();
                _indexFS.Close();
                _indexFS.Dispose();
                _indexFS = null;
            }
        }

        public List<BusinessCode> ReadAll()
        {
            _sempaphore.Wait();
            try
            {
                List<BusinessCode> result = new List<BusinessCode>(20);
                byte[] bytes = new byte[BusinessCode.BYTES_LENGTH];
                foreach (var index in _indexQueue.ToArray())
                {
                    _dataFS.Seek(index.Position, SeekOrigin.Begin);
                    _dataFS.Read(bytes, 0, bytes.Length);
                    var code = new BusinessCode(bytes);
                    if (code.ApplicationId != index.ApplicationID || code.CodeType != index.CodeType)
                    {
                        _logger.LogError($"Data file invalid. index info: applicationId={index.ApplicationID}, codeType={index.CodeType}");
                    }
                    result.Add(code);
                }
                return result;
            }
            catch
            {
                return new List<BusinessCode>();
            }
            finally
            {
                _sempaphore.Release();
            }
        }

        public async Task WriteAsync(BusinessCode codeToFlush, long globalVersion)
        {
            if (codeToFlush == null)
            {
                return;
            }
            _sempaphore.Wait();

            try
            {
                byte[] dataBytes = new byte[BusinessCode.BYTES_LENGTH];
                var code = new BusinessCode(codeToFlush.ToBytes());// 防止2次读取不一致

                var existsCodeIndex = _indexQueue.FirstOrDefault(m => m.ApplicationID == code.ApplicationId && m.CodeType == code.CodeType);
                if (existsCodeIndex == null)
                {
                    _dataFS.Seek(0, SeekOrigin.End);
                    var indexPosition = _dataFS.Position;
                    dataBytes = code.ToBytes();
                    await _dataFS.WriteAsync(dataBytes, 0, dataBytes.Length);

                    var codeIndex = new BusinessCodeIndex
                    {
                        ApplicationID = code.ApplicationId,
                        CodeType = code.CodeType,
                        Position = indexPosition
                    };
                    var codeIndexBytes = codeIndex.ToBytes();
                    await _indexFS.WriteAsync(codeIndexBytes, 0, codeIndexBytes.Length);
                    _indexQueue.Enqueue(codeIndex);
                }
                else
                {
                    _dataFS.Seek(existsCodeIndex.Position, SeekOrigin.Begin);
                    _dataFS.Read(dataBytes, 0, dataBytes.Length);
                    var currentCode = new BusinessCode(dataBytes);
                    if (code.ApplicationId != currentCode.ApplicationId || code.CodeType != currentCode.CodeType)
                    {
                        _logger.LogError($"Bad file {_dataFileName}, applicationId={codeToFlush.ApplicationId}, codeType={codeToFlush.CodeType}");
                        throw new InvalidDataException($"Bad file {_dataFileName}");
                    }
                    if (code.ApplicationId == currentCode.ApplicationId && code.CodeType == currentCode.CodeType)
                    {
                        if (code.VersionId < currentCode.VersionId)
                        {
                            _logger.LogError($"Bad file {_dataFileName}, version is invalid, applicationId={codeToFlush.ApplicationId}, codeType={codeToFlush.CodeType}, versionId={codeToFlush.VersionId}");
                            throw new InvalidDataException($"Bad file {_dataFileName}");
                        }
                        if (code.Infix != currentCode.Infix || code.Sequence != currentCode.Sequence)
                        {
                            if (code.Infix == currentCode.Infix && code.Sequence < currentCode.Sequence)
                            {
                                _logger.LogError($"Bad file {_dataFileName}, sequence is invalid, applicationId={codeToFlush.ApplicationId}, codeType={codeToFlush.CodeType}, sequence={codeToFlush.Sequence}");
                                throw new InvalidDataException($"Bad file {_dataFileName}");
                            }
                            _dataFS.Seek(existsCodeIndex.Position, SeekOrigin.Begin);
                            dataBytes = code.ToBytes();
                            await _dataFS.WriteAsync(dataBytes, 0, dataBytes.Length);
                        }
                    }
                }

                // 写入全局版本号
                _dataFS.Seek(0, SeekOrigin.Begin);
                await _dataFS.WriteAsync(BitConverter.GetBytes(globalVersion), 0, 8);
            }
            catch(Exception ex)
            {
                _logger.LogError($"Write fail: {ex.Message}, applicationId={codeToFlush.ApplicationId}, codeType={codeToFlush.CodeType}", ex);
                throw;
            }
            finally
            {
                _sempaphore.Release();
            }
        }

        public async Task FlushAsync()
        {
            await _dataFS.FlushAsync();
            await _indexFS.FlushAsync();
        }
    }
}
