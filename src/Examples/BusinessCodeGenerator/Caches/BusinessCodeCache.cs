using BusinessCodeGenerator.Storage;
using System;
using System.Collections.Concurrent;

namespace BusinessCodeGenerator.Caches
{
    public class BusinessCodeCache : IBusinessCodeCache
    {
        private readonly ConcurrentDictionary<string, BusinessCode> _businessCodeDict = new ConcurrentDictionary<string, BusinessCode>();

        public BusinessCode Get(Guid applicationId, byte codeType)
        {
            var key = BusinessCode.GetUniqueKeyString(applicationId, codeType);
            return _businessCodeDict.GetOrAdd(key, new BusinessCode(applicationId, codeType));
        }
    }
}
