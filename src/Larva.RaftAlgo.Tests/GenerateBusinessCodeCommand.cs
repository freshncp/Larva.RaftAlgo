using System;

namespace Larva.RaftAlgo.Tests
{
    public class GenerateBusinessCodeCommand
    {
        public GenerateBusinessCodeCommand(Guid applicationId, byte codeType, DateTime? currentTime)
        {
            ApplicationId = applicationId;
            CodeType = codeType;
            CurrentTime = currentTime;
        }
        
        public Guid ApplicationId { get; private set; }
        
        public byte CodeType { get; private set; }
        
        public DateTime? CurrentTime { get; private set; }
    }
}
