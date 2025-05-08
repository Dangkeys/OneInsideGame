using System;
using System.Collections.Generic;
using Mono.Cecil.Cil;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class VoteManagerUI : NetworkBehaviour
{
    [SerializeField][AssetsOnly] private VoteItem voteItemPrefab;
    [SerializeField][SceneObjectsOnly] private Transform voteItemParent;
    [SerializeField][SceneObjectsOnly] private Slider votingTimerSlider;
    private VoteSystem voteManager;
    NetworkPlayerData networkPlayerData;
    void Awake()
    {
        if (OneInsideGameManager.Instance == null)
        {
            Debug.LogWarning("OneInsideGameManager.Instance is null");
            return;
        }
        networkPlayerData = ConnectionManager.Instance.NetworkPlayerData;
    }

    private void Start()
    {
        if (OneInsideLevelSystem.Instance == null)
        {
            Debug.LogWarning("OneInsideLevelManager.Instance is null");
            return;
        }
        voteManager = OneInsideLevelSystem.Instance.VoteSystem;
        voteManager.OnStateChanged += StateChanged;
        voteManager.VotingTimer.OnValueChanged += VotingTimerChanged;
        voteManager.VoteRegistry.OnValueChanged += VoteDictionaryChanged;
        gameObject.SetActive(false);
    }

    private void VoteDictionaryChanged(Dictionary<ulong, ulong> previousValue, Dictionary<ulong, ulong> newValue)
    {
        RefreshVoteItems();
    }

    private void VotingTimerChanged(float previousValue, float newValue)
    {
        votingTimerSlider.value = newValue / voteManager.VotingTimerMax;
    }

    private void StateChanged(VoteState state)
    {
        switch (state)
        {
            case VoteState.WaitingToVote:
                gameObject.SetActive(false);
                break;
            case VoteState.Voting:
                gameObject.SetActive(true);
                break;
            case VoteState.VoteOver:
                gameObject.SetActive(false);
                break;
        }
    }

    private void RefreshVoteItems()
    {
        foreach (Transform child in voteItemParent)
        {
            Destroy(child.gameObject);
        }

        Player localPlayer = PlayerSystem.GetLocalPlayerScript();


        foreach (var vote in voteManager.VoteRegistry.Value)
        {
            VoteItem voteItem = Instantiate(voteItemPrefab, voteItemParent);

            Player player = PlayerSystem.GetPlayerByClientId(vote.Key);

            if (!localPlayer.IsAlive.Value)
            {
                voteItem.Initialise(GetPlayerName(vote.Key), vote.Key, vote.Value != VoteSystem.NO_VOTE, false);
            }
            else
            {
                voteItem.Initialise(GetPlayerName(vote.Key), vote.Key, vote.Value != VoteSystem.NO_VOTE, player.IsAlive.Value);
            }
        }
    }
    private string GetPlayerName(ulong clientId)
    {
        if (networkPlayerData != null &&
            networkPlayerData.ClientIdToAuth.Value.TryGetValue(clientId, out FixedString32Bytes authId) &&
            networkPlayerData.AuthIdToUserData.Value.TryGetValue(authId, out UserDataDto userData))
        {
            if (clientId == NetworkManager.Singleton.LocalClientId)
            {
                return $"{userData.Name} (You)";
            }
            return userData.Name.ToString();
        }

        return clientId == NetworkManager.Singleton.LocalClientId ? "You" : $"Player {clientId}";
    }
}