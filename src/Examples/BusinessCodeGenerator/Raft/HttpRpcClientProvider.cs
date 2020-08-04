using System;
using System.Net.Http;
using Larva.RaftAlgo;
using Larva.RaftAlgo.Concensus.Rpc;
using Microsoft.Extensions.Logging;

namespace BusinessCodeGenerator.Raft
{
    public class HttpRpcClientProvider : IRpcClientProvider
    {
        private readonly int _commandTimeoutMilliseconds;
        private readonly ILoggerFactory _loggerFactory;

        public HttpRpcClientProvider(IRaftSettings settings, ILoggerFactory loggerFactory)
        {
            _commandTimeoutMilliseconds = settings.CommandTimeoutMilliseconds <= 0 ? 1000 : settings.CommandTimeoutMilliseconds;
            _loggerFactory = loggerFactory;
        }

        public IRpcClient GetRpcClient(Uri serviceUri)
        {
            var httpClient = new HttpClient();
            httpClient.BaseAddress = serviceUri;
            httpClient.Timeout = TimeSpan.FromMilliseconds(_commandTimeoutMilliseconds);
            return new HttpRpcClient(httpClient, _loggerFactory);
        }
    }
}