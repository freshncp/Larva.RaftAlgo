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
        private readonly ICommandSerializer _commandSerializer;
        private readonly IBusinessCodeConfigManager _businessCodeConfigManager;
        private readonly IBusinessCodeCache _businessCodeCache;

        public BusinessCodeFiniteStateMachine(
            ICommandSerializer commandSerializer,
            IBusinessCodeConfigManager businessCodeConfigManager,
            IBusinessCodeCache businessCodeCache)
        {
            _commandSerializer = commandSerializer;
            _businessCodeConfigManager = businessCodeConfigManager;
            _businessCodeCache = businessCodeCache;
        }

        public async Task<string> HandleAsync(LogEntry log)
        {
            return await Task.Run(() =>
            {
                if (!string.IsNullOrEmpty(log.CommandType) && log.CommandData != null)
                {
                    var command = _commandSerializer.Deserialize(log.CommandType, log.CommandData);
                    if (command != null && command is GenerateBusinessCodeCommand generateCommand)
                    {
                        var config = _businessCodeConfigManager.Get(generateCommand.ApplicationId, generateCommand.CodeType);
                        if (config == null)
                        {
                            return string.Empty;
                        }

                        var code = _businessCodeCache.Get(generateCommand.ApplicationId, generateCommand.CodeType);
                        code.Generate(generateCommand.CurrentTime ?? DateTime.Now);
                        var generatedCode = $"{config.CodePrefix}{code.Infix}{code.Sequence.ToString().PadLeft(config.SequenceMinLength, '0')}";
                        Console.WriteLine($"generatedCode={generatedCode}");
                        return generatedCode;
                    }
                }
                return string.Empty;
            });
        }
    }
}
