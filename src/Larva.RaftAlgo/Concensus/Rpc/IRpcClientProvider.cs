using System;

namespace Larva.RaftAlgo.Concensus.Rpc
{
    /// <summary>
    /// Rpc client provider
    /// </summary>
    public interface IRpcClientProvider
    {
        /// <summary>
        /// Get rpc client
        /// </summary>
        /// <param name="serviceUri"></param>
        /// <returns></returns>
        IRpcClient GetRpcClient(Uri serviceUri);
    }
}