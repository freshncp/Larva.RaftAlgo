using System;
using System.Threading.Tasks;
using Larva.RaftAlgo.Concensus.Rpc;
using Larva.RaftAlgo.Concensus.Rpc.Messages;
using Larva.RaftAlgo.Log;
using Larva.RaftAlgo.StateMachine;

namespace Larva.RaftAlgo.Concensus.Node
{
    /// <summary>
    /// Local node
    /// </summary>
    public class LocalNode : INode, IRpcClient
    {
        private readonly IReplicatedStateMachine _stateMachine;
        private readonly ILogRepository _logRepository;
        private readonly Random _timeoutRandom;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="serviceUri"></param>
        /// <param name="stateMachine"></param>
        /// <param name="logRepository"></param>
        public LocalNode(string id, Uri serviceUri, IReplicatedStateMachine stateMachine, ILogRepository logRepository)
        {
            Id = id;
            ServiceUri = serviceUri;
            Role = NodeRole.Follower;
            _stateMachine = stateMachine;
            _logRepository = logRepository;
            _timeoutRandom = new Random(DateTime.Now.Millisecond);
        }

        /// <summary>
        /// Node's id
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Service Uri
        /// </summary>
        public Uri ServiceUri { get; }

        /// <summary>
        /// Node's state
        /// </summary>
        public NodeState State { get; private set; }

        /// <summary>
        /// Node's role
        /// </summary>
        public NodeRole Role { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public async Task StartUpAsync()
        {
            var lastTermAndIndex = await _logRepository.GetLastTermAndIndexAsync();
            var lastApplied = 0L;
            for (var index = 1; index <= lastTermAndIndex.index; index++)
            {
                var log = await _logRepository.GetLogEntryAsync(index);
                await _stateMachine.HandleAsync(log);
                ++lastApplied;
            }
            State = new NodeState(lastTermAndIndex.term, null, lastTermAndIndex.index, lastApplied);
            await StartElectionAsync();
        }

        private async Task StartElectionAsync()
        {
            await Task.Delay(_timeoutRandom.Next(30, 60) * 100).ContinueWith(async (lastTask, state) =>
            {
                // Follower election timeout
                if (Role == NodeRole.Follower && string.IsNullOrEmpty(State.VotedFor))
                {
                    BecomeCandidate();
                }

                // Candidate new election timeout
                if (Role == NodeRole.Candidate)
                {
                    await StartElectionAsync();
                    //TODO:Election
                }
            }, this);
        }

        /// <summary>
        /// Become follower
        /// </summary>
        public void BecomeFollower()
        {
            if (Role != NodeRole.Follower)
            {
                Role = NodeRole.Follower;
                Task.Factory.StartNew(StartElectionAsync);
            }
        }

        /// <summary>
        /// Become candidate
        /// </summary>
        public void BecomeCandidate()
        {
            if (Role != NodeRole.Candidate)
            {
                Role = NodeRole.Candidate;
            }
        }

        /// <summary>
        /// Become leader
        /// </summary>
        public void BecomeLeader()
        { }

        #region Receive RPC message

        /// <summary>
        /// Receive RequestVote RPC request
        /// </summary>
        public Task<RequestVoteResponse> RequestVote(RequestVoteRequest request)
        {
            // 1. Reply false if term < currentTerm (§5.1)

            // 2. If votedFor is null or candidateId, and candidate’s log is at
            // least as up-to-date as receiver’s log, grant vote (§5.2, §5.4)

            throw new NotImplementedException();
        }

        /// <summary>
        /// Receive AppendEntries RPC request
        /// </summary>
        public Task<AppendEntriesResponse> AppendEntries(AppendEntriesRequest request)
        {
            // 1. Reply false if term < currentTerm (§5.1)

            // 2. Reply false if log doesn’t contain an entry at prevLogIndex
            // whose term matches prevLogTerm (§5.3)

            // 3. If an existing entry conflicts with a new one (same index
            // but different terms), delete the existing entry and all that
            // follow it (§5.3)

            // 4. Append any new entries not already in the log

            // 5. If leaderCommit > commitIndex, set commitIndex =
            // min(leaderCommit, index of last new entry)

            throw new NotImplementedException();
        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="command"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public Task<ExecuteCommandResponse> ExecuteCommand<T>(T command)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Query cluster info
        /// </summary>
        /// <returns></returns>
        public Task<QueryClusterInfoResponse> QueryClusterInfo()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add node to cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AddNodeToClusterResponse> AddNodeToCluster(AddNodeToClusterRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Rules

        #endregion
    }
}