using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;

public class VoteManager : NetworkBehaviour
{
    public enum State
    {
        WaitingToVote,
        Voting,
        VoteOver,
    }

    public event Action<State> OnStateChanged;

    [field: SerializeField] public float VotingTimerMax = 60f;
    public NetworkVariable<float> VotingTimer = new NetworkVariable<float>();
    private NetworkVariable<State> state = new NetworkVariable<State>(State.WaitingToVote);
    public NetworkVariable<Dictionary<ulong, ulong>> VoteRegistry { get; private set; } =
        new NetworkVariable<Dictionary<ulong, ulong>>(new Dictionary<ulong, ulong>());

    public const ulong NO_VOTE = ulong.MaxValue;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            VotingTimer.Value = VotingTimerMax;
        }

        state.OnValueChanged += StateChanged;
        VoteRegistry.OnValueChanged += VoteDictionaryChanged;
    }
    private void Update()
    {
        if (!IsServer || state.Value != State.Voting)
            return;

        UpdateVotingTimer();
    }



    public override void OnNetworkDespawn()
    {
        state.OnValueChanged -= StateChanged;
        VoteRegistry.OnValueChanged -= VoteDictionaryChanged;
    }
    private void UpdateVotingTimer()
    {
        if (VotingTimer.Value <= 0)
            return;

        VotingTimer.Value -= Time.deltaTime;

        if (VotingTimer.Value < 0)
        {
            state.Value = State.VoteOver;
        }
    }
    private void VoteDictionaryChanged(Dictionary<ulong, ulong> previousValue, Dictionary<ulong, ulong> newValue)
    {
        if (!IsServer)
            return;

        bool allVoted = newValue.Values.All(vote => vote != NO_VOTE);
        if (allVoted && state.Value == State.Voting)
        {
            state.Value = State.VoteOver;
        }
    }

    private void StateChanged(State previousValue, State newState)
    {
        OnStateChanged?.Invoke(newState);
        if (!IsServer)
            return;

        switch (newState)
        {
            case State.Voting:
                InitializeVotingDictionary();
                break;
            case State.VoteOver:
                ProcessVoteResults();
                state.Value = State.WaitingToVote;
                break;
        }

    }


    private void InitializeVotingDictionary()
    {
        Dictionary<ulong, ulong> newDictionary = NetworkManager.Singleton.ConnectedClientsIds
            .ToDictionary(clientId => clientId, _ => NO_VOTE);
        VoteRegistry.Value = newDictionary;
    }



    private void ProcessVoteResults()
    {
        Dictionary<ulong, ulong> voteResults = CalculateVoteResults();
        ulong highestVoteCount = FindHighestVoteCount(voteResults);
        List<ulong> mostVotedPlayers = FindMostVotedPlayers(voteResults, highestVoteCount);
        LogVoteResults(highestVoteCount, mostVotedPlayers);
    }

    private Dictionary<ulong, ulong> CalculateVoteResults()
    {
        Dictionary<ulong, ulong> voteResults = new Dictionary<ulong, ulong>();

        foreach (KeyValuePair<ulong, ulong> vote in VoteRegistry.Value.Where(v => v.Value != NO_VOTE))
        {
            if (voteResults.ContainsKey(vote.Value))
            {
                voteResults[vote.Value]++;
            }
            else
            {
                voteResults[vote.Value] = 1;
            }
        }

        return voteResults;
    }

    private ulong FindHighestVoteCount(Dictionary<ulong, ulong> voteResults)
    {
        if (!voteResults.Any())
        {
            return 0;
        }
        return voteResults.Max(v => v.Value);
    }

    private List<ulong> FindMostVotedPlayers(Dictionary<ulong, ulong> voteResults, ulong highestVoteCount)
    {
        if (!voteResults.Any())
        {
            return new List<ulong>();
        }

        return voteResults
            .Where(v => v.Value == highestVoteCount)
            .Select(v => v.Key)
            .ToList();
    }

    private void LogVoteResults(ulong highestVoteCount, List<ulong> mostVotedPlayers)
    {
        switch (mostVotedPlayers.Count)
        {
            case 0:
                Debug.Log("No votes were cast");
                break;
            case 1:
                Debug.Log($"Player {mostVotedPlayers[0]} has been voted to be the impostor by {highestVoteCount} players");
                break;
            default:
                string tiedPlayers = string.Join(", ", mostVotedPlayers);
                Debug.Log($"Tie vote! Players {tiedPlayers} each received {highestVoteCount} votes");
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RaiseVoteStartServerRpc()
    {
        if (state.Value != State.Voting)
        {
            VotingTimer.Value = VotingTimerMax;
            state.Value = State.Voting;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void VoteTargetPlayerServerRpc(ulong clientId, ServerRpcParams serverRpcParams = default)
    {
        ulong votingPlayerId = serverRpcParams.Receive.SenderClientId;

        if (votingPlayerId == clientId)
        {
            Debug.LogWarning($"Player {votingPlayerId} attempted to vote for themselves");
            return;
        }

        Dictionary<ulong, ulong> newDictionary = new Dictionary<ulong, ulong>(VoteRegistry.Value)
        {
            [votingPlayerId] = clientId
        };
        VoteRegistry.Value = newDictionary;
    }
}