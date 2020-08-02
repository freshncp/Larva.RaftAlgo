using System;
using Larva.RaftAlgo.Concensus.Rpc;

namespace BusinessCodeGenerator.Raft
{
    public class HttpRpcClientProvider : IRpcClientProvider
    {
        public IRpcClient GetRpcClient(Uri serviceUri)
        {
            throw new NotImplementedException();
        }
    }
}