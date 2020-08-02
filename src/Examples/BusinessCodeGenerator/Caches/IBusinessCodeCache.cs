using System;
using BusinessCodeGenerator.Storage;

namespace BusinessCodeGenerator.Caches
{
    public interface IBusinessCodeCache
    {
        BusinessCode Get(Guid applicationId, byte codeType);
    }
}
