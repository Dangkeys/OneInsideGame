using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class VoteSystem : NetworkBehaviour
{

    public event Action<VoteState> OnStateChanged;

    [field: SerializeField] public float VotingTimerMax = 60f;
    public NetworkVariable<float> VotingTimer = new NetworkVariable<float>();
    private NetworkVariable<VoteState> state = new NetworkVariable<VoteState>(VoteState.WaitingToVote);

    //Key is the clientId of the player who is voting
    //Value is the clientId of the player who is being voted for
    public NetworkVariable<Dictionary<ulong, ulong>> VoteRegistry { get; private set; } =
        new NetworkVariable<Dictionary<ulong, ulong>>(new Dictionary<ulong, ulong>());

    NetworkPlayerData networkPlayerData;


    public const ulong NO_VOTE = ulong.MaxValue;

    void Awake()
    {
        if (OneInsideGameManager.Instance == null)
        {
            Debug.LogWarning("OneInsideGameManager.Instance is null");
            return;
        }
        networkPlayerData = ConnectionManager.Instance.NetworkPlayerData;
    }

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
        if (!IsServer || state.Value != VoteState.Voting)
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
            state.Value = VoteState.VoteOver;
        }
    }
    private void VoteDictionaryChanged(Dictionary<ulong, ulong> previousValue, Dictionary<ulong, ulong> newValue)
    {
        if (!IsServer)
            return;

        bool allVoted = newValue.Values.All(vote => vote != NO_VOTE);
        if (allVoted && state.Value == VoteState.Voting)
        {
            state.Value = VoteState.VoteOver;
        }
    }

    private void StateChanged(VoteState previousValue, VoteState newState)
    {
        OnStateChanged?.Invoke(newState);
        if (!IsServer)
            return;

        switch (newState)
        {
            case VoteState.Voting:
                InitializeVotingDictionary();
                break;
            case VoteState.VoteOver:
                ProcessVoteResults();
                state.Value = VoteState.WaitingToVote;
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

        foreach (KeyValuePair<ulong, ulong> vote in VoteRegistry.Value)
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
                UIManager.Instance.ShowMessage("No votes were cast");
                break;
            case 1:
                if (mostVotedPlayers[0] == NO_VOTE)
                {
                    UIManager.Instance.ShowMessage("Tie vote!");
                }
                else
                {
                    UIManager.Instance.ShowMessage($"{GetPlayerName(mostVotedPlayers[0])} has been voted to be the impostor by {highestVoteCount} players");
                }
                break;
            default:
                UIManager.Instance.ShowMessage("Tie vote!");
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RaiseVoteStartServerRpc()
    {
        if (OneInsideLevelSystem.Instance.State.Value != GameState.GamePlaying)
        {
            Debug.LogWarning("Attempted to start a vote while not in game");
            return;
        }
        if (state.Value != VoteState.Voting)
        {
            VotingTimer.Value = VotingTimerMax;
            state.Value = VoteState.Voting;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void VoteTargetPlayerServerRpc(ulong targetClientId, ServerRpcParams serverRpcParams = default)
    {
        ulong voterClientId = serverRpcParams.Receive.SenderClientId;
        Player voter = PlayerSystem.GetPlayerByClientId(voterClientId);
        Player targetPlayer = PlayerSystem.GetPlayerByClientId(targetClientId);
        if (voterClientId == targetClientId)
        {
            Debug.LogWarning($"Player {GetPlayerName(voterClientId)} attempted to vote for themselves");
            return;
        }

        if (!voter.IsAlive.Value || !targetPlayer.IsAlive.Value)
        {
            Debug.LogWarning($"Player {GetPlayerName(voterClientId)} attempted to vote for {GetPlayerName(targetClientId)} while dead");
            return;
        }

        Dictionary<ulong, ulong> newDictionary = new Dictionary<ulong, ulong>(VoteRegistry.Value)
        {
            [voterClientId] = targetClientId
        };
        VoteRegistry.Value = newDictionary;
    }
    private string GetPlayerName(ulong clientId)
    {

        NetworkPlayerData networkPlayerData = ConnectionManager.Instance.NetworkPlayerData;
        UserDataDto userData = networkPlayerData.GetUserDataFromClientId(clientId);
        
        if (userData == null)
        {
            return clientId == NetworkManager.Singleton.LocalClientId ? "You" : $"Player {clientId}";
        }

        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            return $"{userData.Name} (You)";
        }
        return userData.Name.ToString();
    }
}