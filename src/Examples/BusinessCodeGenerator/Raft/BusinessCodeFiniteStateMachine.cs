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
                var command = Newtonsoft.Json.JsonConvert.DeserializeObject(log.Command);
                if (command != null && command is GenerateBusinessCodeCommand generateCommand)
                {
                    var config = _businessCodeConfigManager.Get(generateCommand.ApplicationId, generateCommand.CodeType);
                    if (config == null)
                    {
                        throw new ApplicationException($"unable to save business code to state machine");
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
