using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Larva.RaftAlgo.Concensus.Cluster;
using Larva.RaftAlgo.Concensus.Rpc.Messages;
using Larva.RaftAlgo.Log;
using Larva.RaftAlgo.StateMachine;

namespace Larva.RaftAlgo.Concensus.Node
{
    /// <summary>
    /// Local node
    /// </summary>
    public sealed class LocalNode : INode
    {
        private readonly IReplicatedStateMachine _stateMachine;
        private readonly ILog _log;
        private readonly IElectionTimeoutRandom _electionTimeoutRandom;
        private readonly int _heartbeatMilliseconds;
        private readonly int _commandTimeoutMilliseconds;
        private readonly int _appendEntriesBatchSize;
        private readonly SemaphoreSlim _nodeLock = new SemaphoreSlim(1, 1);
        private ICluster _cluster;
        private Timer _electionTimeoutTimer;
        private volatile int _receivedAppendEntriesOrGrantedVote = 0;
        private volatile int _heartbeatSending = 0;
        private volatile string _leaderId;
        private volatile int _disposing;

        /// <summary>
        /// Local node
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="stateMachine"></param>
        /// <param name="log"></param>
        /// <param name="electionTimeoutRandom"></param>
        /// <param name="settings"></param>
        public LocalNode(
            NodeId nodeId,
            IReplicatedStateMachine stateMachine,
            ILog log,
            IElectionTimeoutRandom electionTimeoutRandom,
            IRaftSettings settings)
        {
            Id = nodeId.Id;
            ServiceUri = nodeId.ServiceUri;
            Role = NodeRole.Follower;
            _stateMachine = stateMachine;
            _log = log;
            _electionTimeoutRandom = electionTimeoutRandom;
            _heartbeatMilliseconds = settings.HeartbeatMilliseconds <= 0 ? 100 : settings.HeartbeatMilliseconds;
            _commandTimeoutMilliseconds = settings.CommandTimeoutMilliseconds <= 0 ? 3000 : settings.CommandTimeoutMilliseconds;
            _appendEntriesBatchSize = settings.AppendEntriesBatchSize <= 0 ? 100 : settings.AppendEntriesBatchSize;
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
        /// Is remote node
        /// </summary>
        public bool IsRemote => false;

        /// <summary>
        /// Start up
        /// </summary>
        /// <param name="cluster"></param>
        /// <returns></returns>
        public async Task StartUpAsync(ICluster cluster)
        {
            _cluster = cluster;
            var lastTermAndIndex = await _log.GetLastTermAndIndexAsync();
            var lastApplied = 0L;
            for (var index = 1; index <= lastTermAndIndex.index; index++)
            {
                var logEntry = await _log.GetAsync(index);
                await _stateMachine.HandleAsync(logEntry);
                ++lastApplied;
            }
            State = new NodeState(lastTermAndIndex.term, null, lastTermAndIndex.index, lastApplied);
            StartElection();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task ShutdownAsync()
        {
            if (Interlocked.CompareExchange(ref _disposing, 1, 0) == 0)
            {
                //TODO: Wait until disposed
                await Task.Delay(1000);
            }
        }

        #region Role Changing

        private void BecomeFollower()
        {
            if (Role != NodeRole.Follower)
            {
                Role = NodeRole.Follower;
                State = new NodeState(State.CurrentTerm, null, State.CommitIndex, State.LastApplied);

                StartElection();
            }
        }

        private void BecomeCandidate()
        {
            if (Role == NodeRole.Follower)
            {
                Role = NodeRole.Candidate;
            }
        }

        private void BecomeLeader()
        {
            if (Role == NodeRole.Candidate)
            {
                Role = NodeRole.Leader;
                var nextIndex = new Dictionary<string, long>();
                var matchIndex = new Dictionary<string, long>();
                State = new LeaderState(State, nextIndex, matchIndex);
            }
        }

        #endregion

        #region Receive RPC message

        /// <summary>
        /// Receive RequestVote RPC request
        /// </summary>
        public async Task<RequestVoteResponse> RequestVoteAsync(RequestVoteRequest request)
        {
            await _nodeLock.WaitAsync();
            var response = new RequestVoteResponse(State.CurrentTerm, false, "Unknown error");
            try
            {
                if (request.Term < State.CurrentTerm)
                {
                    // 1. Reply false if term < currentTerm (§5.1)
                    response = new RequestVoteResponse(State.CurrentTerm, false, "term < currentTerm");
                }
                else
                {
                    var lastTermAndIndex = await _log.GetLastTermAndIndexAsync();
                    if (request.Term > State.CurrentTerm)
                    {
                        // If RPC request or response contains term T > currentTerm: set currentTerm = T, convert to follower (§5.1)
                        State.SetTerm(request.Term);
                        BecomeFollower();
                    }

                    if (Role == NodeRole.Follower
                        && (string.IsNullOrEmpty(State.VotedFor) || State.VotedFor == request.CandidateId))
                    {
                        if ((request.LastLogTerm == lastTermAndIndex.term && request.LastLogIndex == lastTermAndIndex.index)
                            || (request.LastLogTerm >= lastTermAndIndex.term && request.LastLogIndex > lastTermAndIndex.index))
                        {
                            // 2. If votedFor is null or candidateId, and candidate’s log is at
                            // least as up-to-date as receiver’s log, grant vote (§5.2, §5.4)
                            Interlocked.Exchange(ref _receivedAppendEntriesOrGrantedVote, 1);
                            State.VoteFor(request.CandidateId);
                            response = new RequestVoteResponse(State.CurrentTerm, true);
                        }
                        else
                        {
                            Console.WriteLine($"request.LastLogTerm={request.LastLogTerm}, request.LastLogIndex={request.LastLogIndex}");
                            Console.WriteLine($"lastTermAndIndex.term={lastTermAndIndex.term}, lastTermAndIndex.index={lastTermAndIndex.index}");
                            response = new RequestVoteResponse(State.CurrentTerm, false, "candidate’s log is not up-to-date as receiver’s log");
                        }
                    }
                    else
                    {
                        response = new RequestVoteResponse(State.CurrentTerm, false, "Not follower or has grant vote to another candicate");
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handle RequestVoteAsync fail: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
            finally
            {
                _nodeLock.Release();
            }
        }

        /// <summary>
        /// Receive AppendEntries RPC request
        /// </summary>
        public async Task<AppendEntriesResponse> AppendEntriesAsync(AppendEntriesRequest request)
        {
            await _nodeLock.WaitAsync();
            var response = new AppendEntriesResponse(State.CurrentTerm, false);
            Interlocked.Exchange(ref _receivedAppendEntriesOrGrantedVote, 1);
            try
            {
                var termAtPrevLogIndex = await _log.GetTermAtIndexAsync(request.PrevLogIndex);
                if (request.Term < State.CurrentTerm)
                {
                    // 1. Reply false if term < currentTerm (§5.1)
                    response = new AppendEntriesResponse(State.CurrentTerm, false);
                }
                else if (termAtPrevLogIndex > 0 && termAtPrevLogIndex != request.PrevLogTerm)
                {
                    // 2. Reply false if log doesn’t contain an entry at prevLogIndex
                    // whose term matches prevLogTerm (§5.3)
                    response = new AppendEntriesResponse(State.CurrentTerm, false);
                }
                else
                {
                    if (request.Term > State.CurrentTerm)
                    {
                        // If RPC request or response contains term T > currentTerm: set currentTerm = T, convert to follower (§5.1)
                        State.SetTerm(request.Term);
                        BecomeFollower();
                    }
                    if (Role == NodeRole.Candidate)
                    {
                        // If AppendEntries RPC received from new leader: convert to follower
                        BecomeFollower();
                    }

                    if (request.Entries != null && request.Entries.Length > 0)
                    {
                        // 3. If an existing entry conflicts with a new one (same index
                        // but different terms), delete the existing entry and all that
                        // follow it (§5.3)
                        await _log.DeleteConflictsFromThisLogAsync(request.PrevLogIndex, request.Entries);

                        // 4. Append any new entries not already in the log
                        var lastTermAndIndex = await _log.GetLastTermAndIndexAsync();
                        var lastIndexAheadCount = Convert.ToInt32(lastTermAndIndex.index - request.PrevLogIndex);
                        if (lastIndexAheadCount >= 0 && lastIndexAheadCount < request.Entries.Length)
                        {
                            await _log.BatchAppendAsync(request.Entries.Skip(lastIndexAheadCount).ToArray());
                        }

                        // 5. If leaderCommit > commitIndex, set commitIndex =
                        // min(leaderCommit, index of last new entry)
                        lastTermAndIndex = await _log.GetLastTermAndIndexAsync();
                        State.SetCommitIndexAndLastApplied(Math.Min(request.LeaderCommit, lastTermAndIndex.index));

                    }
                    Interlocked.Exchange(ref _leaderId, request.LeaderId);
                    response = new AppendEntriesResponse(State.CurrentTerm, true);
                }

                return response;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Handle AppendEntriesAsync fail: {ex.Message}\n{ex.StackTrace}");
                throw;
            }
            finally
            {
                _nodeLock.Release();
            }
        }

        /// <summary>
        /// Execute command
        /// </summary>
        /// <param name="command"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public async Task<ExecuteCommandResponse> ExecuteCommandAsync<T>(T command)
        {
            await _nodeLock.WaitAsync();
            var response = new ExecuteCommandResponse(null, false, "Unknown error");
            switch (Role)
            {
                case NodeRole.Follower:
                    if (string.IsNullOrEmpty(_leaderId))
                    {
                        response = new ExecuteCommandResponse(null, false, "No leader found");
                        _nodeLock.Release();
                    }
                    else
                    {
                        var leaderNode = _cluster.Nodes?.FirstOrDefault(f => f.Id == _leaderId);
                        if (leaderNode == null)
                        {
                            response = new ExecuteCommandResponse(null, false, $"Leader {_leaderId} not found in cluster");
                            _nodeLock.Release();
                        }
                        else
                        {
                            _nodeLock.Release();
                            response = await leaderNode.ExecuteCommandAsync(command);
                        }
                    }
                    break;
                case NodeRole.Candidate:
                    response = new ExecuteCommandResponse(null, false, $"Candidate couldn't execute command");
                    _nodeLock.Release();
                    break;
                case NodeRole.Leader:
                    if (_cluster.IsSingleNodeCluster())
                    {
                        var newLogEntry = new LogEntry(command, State.CurrentTerm);
                        await _log.AppendAsync(newLogEntry);
                        var executeResult = await _stateMachine.HandleAsync(newLogEntry);
                        response = new ExecuteCommandResponse(executeResult, true, null);
                        var lastTermAndIndex = await _log.GetLastTermAndIndexAsync();
                        State.SetCommitIndexAndLastApplied(lastTermAndIndex.index);
                        _nodeLock.Release();
                    }
                    else
                    {
                        var newLogEntry = new LogEntry(command, State.CurrentTerm);
                        await _log.AppendAsync(newLogEntry);
                        var executeResult = await _stateMachine.HandleAsync(newLogEntry);
                        response = new ExecuteCommandResponse(executeResult, true, null);
                        var lastTermAndIndex = await _log.GetLastTermAndIndexAsync();
                        State.SetCommitIndexAndLastApplied(lastTermAndIndex.index);
                        _nodeLock.Release();
                        // If command received from client: append entry to local log, respond after entry applied to state machine (§5.3)
                        //TODO: Replicated log to followers.
                    }
                    break;
            }
            return response;
        }

        /// <summary>
        /// Query node info
        /// </summary>
        /// <returns></returns>
        public Task<QueryNodeInfoResponse> QueryNodeInfoAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Query cluster info
        /// </summary>
        /// <returns></returns>
        public Task<QueryClusterInfoResponse> QueryClusterInfoAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Add node to cluster
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public Task<AddNodeToClusterResponse> AddNodeToClusterAsync(AddNodeToClusterRequest request)
        {
            throw new NotImplementedException();
        }

        #endregion

        private void StartElection()
        {
            _electionTimeoutTimer?.Dispose();

            var electionTimeout = _electionTimeoutRandom.GetElectionTimeout();
            Console.WriteLine($"Election timeout after {electionTimeout.TotalMilliseconds}");
            _electionTimeoutTimer = new Timer(async (state) =>
            {
                if (_disposing == 1) return;

                Console.WriteLine($"Election timeout.");
                await _nodeLock.WaitAsync();
                if (Role == NodeRole.Follower)
                {
                    if (_receivedAppendEntriesOrGrantedVote == 0 || string.IsNullOrEmpty(State.VotedFor))
                    {
                        // If election timeout elapses without receiving AppendEntries
                        // RPC from current leader or granting vote to candidate: convert to candidate
                        BecomeCandidate();
                    }
                    else
                    {
                        Interlocked.Exchange(ref _receivedAppendEntriesOrGrantedVote, 0);
                    }
                    StartElection();
                    _nodeLock.Release();
                }
                else if (Role == NodeRole.Candidate)
                {
                    // Increment currentTerm
                    // Vote for self
                    State.IncreaseTermAndVoteFor(Id);
                    _nodeLock.Release();

                    await DoElectionAsync();
                }
                else
                {
                    _electionTimeoutTimer?.Dispose();
                    _nodeLock.Release();
                }
            }, this, electionTimeout, electionTimeout);

            // Task.Delay(electionTimeout).ContinueWith(async (lastTask, state) =>
            // {
            // }, this);
        }

        private async Task DoElectionAsync()
        {
            var getVotes = new List<Task>();
            if (_cluster.IsSingleNodeCluster())
            {
                BecomeLeader();
            }
            else
            {
                var receiveFromMajorityServers = false;
                var currentTerm = State.CurrentTerm;
                var lastTermAndIndex = await _log.GetLastTermAndIndexAsync();
                var requestVoteTaskList = new List<Task<RequestVoteResponse>>();
                foreach (var remoteNode in _cluster.Nodes.Where(w => w.IsRemote))
                {
                    requestVoteTaskList.Add(remoteNode.RequestVoteAsync(new RequestVoteRequest(currentTerm, Id, lastTermAndIndex.index, lastTermAndIndex.term)));
                }

                var completedTaskIdList = new List<int>();
                int voteCount = 1;
                while (requestVoteTaskList.Count > completedTaskIdList.Count || _disposing == 1)
                {
                    foreach (var task in requestVoteTaskList.FindAll(f => !completedTaskIdList.Contains(f.Id)))
                    {
                        if (task.IsCompleted)
                        {
                            completedTaskIdList.Add(task.Id);
                            if (task.Exception == null && task.Result.VoteGranted)
                            {
                                ++voteCount;
                            }
                        }
                    }

                    if (voteCount > _cluster.GetNodeCount() / 2)
                    {
                        receiveFromMajorityServers = true;
                        break;
                    }
                }

                await _nodeLock.WaitAsync();
                if (Role == NodeRole.Candidate)
                {
                    if (receiveFromMajorityServers)
                    {
                        // If votes received from majority of servers: become leader
                        BecomeLeader();
                        StartSendingHeartbeat();
                    }
                    else
                    {
                        // If election timeout elapses: start new election
                        StartElection();
                    }
                }
                _nodeLock.Release();
            }
        }

        private void StartSendingHeartbeat()
        {
            Task.Delay(_heartbeatMilliseconds).ContinueWith(async (lastTask, state) =>
            {
                if (_disposing == 1) return;
                if (Role == NodeRole.Leader)
                {
                    StartSendingHeartbeat();
                    var lastHeartbeatSending = Interlocked.Exchange(ref _heartbeatSending, 1);
                    await SendAppendEntriesAsync(lastHeartbeatSending == 1);
                }
            }, this);
        }

        private async Task SendAppendEntriesAsync(bool lastHeartbeatSending)
        {
            if (_cluster.IsSingleNodeCluster())
            {
                return;
            }
            else
            {
                var waitAppendEntriesResponseStopwatch = new Stopwatch();
                await _nodeLock.WaitAsync();
                try
                {
                    if (Role == NodeRole.Leader)
                    {
                        var currentState = State as LeaderState;
                        var lastTermAndIndex = await _log.GetLastTermAndIndexAsync();
                        var sendHeartbeatTasks = new List<Tuple<string, int, Task<AppendEntriesResponse>>>();
                        foreach (var remoteNode in _cluster.Nodes.Where(w => w.IsRemote))
                        {
                            if (!currentState.NextIndex.ContainsKey(remoteNode.Id))
                            {
                                currentState.NextIndex.Add(remoteNode.Id, lastTermAndIndex.index + 1);
                            }
                            if (!currentState.MatchIndex.ContainsKey(remoteNode.Id))
                            {
                                currentState.MatchIndex.Add(remoteNode.Id, 0L);
                            }
                            var prevLogIndex = currentState.MatchIndex[remoteNode.Id];
                            var prevLogTerm = await _log.GetTermAtIndexAsync(prevLogIndex);
                            LogEntry[] entries = null;
                            if (!lastHeartbeatSending)
                            {
                                // If last log index ≥ nextIndex for a follower: send AppendEntries RPC with log entries starting at nextIndex
                                entries = await _log.GetListFromAsync(currentState.MatchIndex[remoteNode.Id] + 1, _appendEntriesBatchSize);
                            }

                            sendHeartbeatTasks.Add(Tuple.Create(remoteNode.Id, entries == null ? 0 : entries.Length, remoteNode.AppendEntriesAsync(new AppendEntriesRequest(currentState.CurrentTerm, Id, prevLogIndex, prevLogTerm, entries, lastTermAndIndex.index))));
                        }

                        var allTaskCompletedTask = Task.WhenAll(sendHeartbeatTasks.Select(s => s.Item3)).ContinueWith((lastTask, state) =>
                        {
                            Interlocked.Exchange(ref _heartbeatSending, 0);
                        }, this);

                        if (!lastHeartbeatSending)
                        {
                            waitAppendEntriesResponseStopwatch.Start();
                            var completedTaskIdList = new List<int>();
                            while (sendHeartbeatTasks.Count > completedTaskIdList.Count && waitAppendEntriesResponseStopwatch.ElapsedMilliseconds < _heartbeatMilliseconds || _disposing == 1)
                            {
                                foreach (var nodeIdAndEntryCountAndTask in sendHeartbeatTasks.Where(f => !completedTaskIdList.Contains(f.Item3.Id)))
                                {
                                    var nodeId = nodeIdAndEntryCountAndTask.Item1;
                                    var entryCount = nodeIdAndEntryCountAndTask.Item2;
                                    var task = nodeIdAndEntryCountAndTask.Item3;
                                    if (task.IsCompleted)
                                    {
                                        completedTaskIdList.Add(task.Id);
                                        if (task.Exception == null)
                                        {
                                            if (task.Result.Success)
                                            {
                                                // If successful: update nextIndex and matchIndex for
                                                // follower (§5.3)
                                                currentState.NextIndex[nodeId] = lastTermAndIndex.index + 1;
                                                currentState.MatchIndex[nodeId] = currentState.MatchIndex[nodeId] + entryCount;

                                            }
                                            else
                                            {
                                                // If AppendEntries fails because of log inconsistency:
                                                // decrement nextIndex and retry (§5.3)
                                                currentState.NextIndex[nodeId] = lastTermAndIndex.index + 1;
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"SendAppendEntriesAsync fail: {ex.Message}\n{ex.StackTrace}");
                }
                finally
                {
                    waitAppendEntriesResponseStopwatch.Reset();
                    _nodeLock.Release();
                }
            }
        }
    }
}