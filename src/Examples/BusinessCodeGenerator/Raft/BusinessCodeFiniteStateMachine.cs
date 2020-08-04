using System;
using System.Threading.Tasks;
using BusinessCodeGenerator.Caches;
using BusinessCodeGenerator.Configuration;
using Larva.RaftAlgo.Log;
using Larva.RaftAlgo.StateMachine;

namespace BusinessCodeGenerator.Raft
{
    public class BusinessCodeFiniteStateMachine : IReplicatedStateMachine
    {
        private readonly IBusinessCodeConfigManager _businessCodeConfigManager;
        private readonly IBusinessCodeCache _businessCodeCache;

        public BusinessCodeFiniteStateMachine(
            IBusinessCodeConfigManager businessCodeConfigManager,
            IBusinessCodeCache businessCodeCache)
        {
            _businessCodeConfigManager = businessCodeConfigManager;
            _businessCodeCache = businessCodeCache;
        }

        public async Task<string> HandleAsync(LogEntry log)
        {
            return await Task.Run(() =>
            {
                if (log.Command != null && log.Command is GenerateBusinessCodeCommand generateCommand)
                {
                    var config = _businessCodeConfigManager.Get(generateCommand.ApplicationId, generateCommand.CodeType);
                    if (config == null)
                    {
                        return string.Empty;
                    }

                    var code = _businessCodeCache.Get(generateCommand.ApplicationId, generateCommand.CodeType);
                    code.Generate(generateCommand.CurrentTime ?? DateTime.Now);
                    var generatedCode = $"{config.CodePrefix}{code.Infix}{code.Sequence.ToString().PadLeft(config.SequenceMinLength, '0')}";
                    return generatedCode;
                }
                return string.Empty;
            });
        }
    }
}
