using System;
using System.Net.Http;
using Larva.RaftAlgo;
using Larva.RaftAlgo.Concensus.Rpc;

namespace BusinessCodeGenerator.Raft
{
    public class HttpRpcClientProvider : IRpcClientProvider
    {
        private readonly int _commandTimeoutMilliseconds;

        public HttpRpcClientProvider(IRaftSettings settings)
        {
            _commandTimeoutMilliseconds = settings.CommandTimeoutMilliseconds <= 0 ? 1000 : settings.CommandTimeoutMilliseconds;
        }

        public IRpcClient GetRpcClient(Uri serviceUri)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = serviceUri;
            httpClient.Timeout = TimeSpan.FromMilliseconds(_commandTimeoutMilliseconds);
            return new HttpRpcClient(httpClient);
        }
    }
}