namespace Larva.RaftAlgo.Concensus.Rpc.Messages
{
    /// <summary>
    /// 
    /// </summary>
    public class ExecuteCommandResponse
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="result"></param>
        /// <param name="isSuccess"></param>
        /// <param name="errorMsg"></param>
        /// <param name="stackTrace"></param>
        public ExecuteCommandResponse(string result,bool isSuccess, string errorMsg, string stackTrace = "")
        {
            Result = result;
            IsSuccess = isSuccess;
            ErrorMsg = errorMsg;
            StackTrace = stackTrace;
        }

        /// <summary>
        /// Result
        /// </summary>
        public string Result { get; private set; }
        
        /// <summary>
        /// Success or fail
        /// </summary>
        public bool IsSuccess { get; private set; }

        /// <summary>
        /// Error message
        /// </summary>
        public string ErrorMsg { get; private set; }

        /// <summary>
        /// Stack trace when fail
        /// </summary>
        public string StackTrace { get; private set; }
    }
}