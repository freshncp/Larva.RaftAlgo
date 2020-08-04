using System.Collections.Generic;

namespace Larva.RaftAlgo.Concensus.Node
{
    /// <summary>
    /// Node state
    /// </summary>
    public class NodeState
    {
        /// <summary>
        /// Node state
        /// </summary>
        /// <param name="currentTerm"></param>
        /// <param name="votedFor"></param>
        /// <param name="commitIndex"></param>
        /// <param name="lastApplied"></param>
        public NodeState(long currentTerm, string votedFor, long commitIndex, long lastApplied)
        {
            CurrentTerm = currentTerm;
            VotedFor = votedFor;
            CommitIndex = commitIndex;
            LastApplied = lastApplied;
        }

        /// <summary>
        /// latest term server has seen (initialized to 0 on first boot, increases monotonically)
        /// </summary>
        public long CurrentTerm { get; private set; }

        /// <summary>
        /// candidateId that received vote in current term (or null if none)
        /// </summary>
        public string VotedFor { get; private set; }

        /// <summary>
        /// index of highest log entry known to be committed (initialized to 0, increases monotonically)
        /// </summary>
        public long CommitIndex { get; private set; }

        /// <summary>
        /// index of highest log entry applied to state machine (initialized to 0, increases monotonically)
        /// </summary>
        public long LastApplied { get; private set; }

        /// <summary>
        /// Increase term and vote for specific node id.
        /// </summary>
        /// <param name="nodeId"></param>
        public void IncreaseTermAndVoteFor(string nodeId)
        {
            ++CurrentTerm;
            VotedFor = nodeId;
        }

        /// <summary>
        /// Set term to greater one
        /// </summary>
        /// <param name="newTerm"></param>
        public void SetTerm(long newTerm)
        {
            if (CurrentTerm < newTerm)
            {
                CurrentTerm = newTerm;
            }
        }

        /// <summary>
        /// Vote for specific node id.
        /// </summary>
        /// <param name="nodeId"></param>
        public void VoteFor(string nodeId)
        {
            if (string.IsNullOrEmpty(VotedFor))
            {
                VotedFor = nodeId;
            }
        }

        /// <summary>
        /// Set commit index to specific one.
        /// </summary>
        /// <param name="newCommitIndex"></param>
        public void SetCommitIndexAndLastApplied(long newCommitIndex)
        {
            CommitIndex = newCommitIndex;
            LastApplied = newCommitIndex;
        }
    }


    /// <summary>
    /// Leader node state
    /// </summary>
    public sealed class LeaderState : NodeState
    {
        /// <summary>
        /// Leader node state
        /// </summary>
        /// <param name="state"></param>
        /// <param name="nextIndex"></param>
        /// <param name="matchIndex"></param>
        public LeaderState(NodeState state,
            IDictionary<string, long> nextIndex, IDictionary<string, long> matchIndex)
            : base(state.CurrentTerm, state.VotedFor, state.CommitIndex, state.LastApplied)
        {
            NextIndex = nextIndex;
            MatchIndex = matchIndex;
        }

        /// <summary>
        /// for each server, index of the next log entry to send to that server (initialized to leader
        /// last log index + 1)
        /// </summary>
        public IDictionary<string, long> NextIndex { get; private set; }

        /// <summary>
        /// for each server, index of highest log entry known to be replicated on server (initialized to 0, increases monotonically)
        /// </summary>
        public IDictionary<string, long> MatchIndex { get; private set; }
    }
}