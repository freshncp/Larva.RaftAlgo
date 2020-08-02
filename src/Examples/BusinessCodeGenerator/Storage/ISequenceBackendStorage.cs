using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BusinessCodeGenerator.Storage
{
    /// <summary>
    /// 序号存储
    /// </summary>
    public interface ISequenceBackendStorage : IDisposable
    {
        List<BusinessCode> ReadAll();
        
        Task<long> GetGlobalVersion();

        Task WriteAsync(BusinessCode codeToFlush, long globalVersion);

        Task FlushAsync();
    }
}
