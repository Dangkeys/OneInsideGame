using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

public class VoteManagerUI : MonoBehaviour
{
    [SerializeField][AssetsOnly] private VoteItem voteItemPrefab;
    [SerializeField][SceneObjectsOnly] private Transform voteItemParent;
    [SerializeField][SceneObjectsOnly] private Slider votingTimerSlider;
    private VoteManager voteManager;

    private void Start()
    {
        if (OneInsideLevelManager.Instance == null)
        {
            Debug.LogError("OneInsideLevelManager.Instance is null");
            return;
        }
        voteManager = OneInsideLevelManager.Instance.VoteManager;
        voteManager.OnStateChanged += StateChanged;
        voteManager.VotingTimer.OnValueChanged += VotingTimerChanged;
        voteManager.VoteRegistry.OnValueChanged += VoteDictionaryChanged;
        gameObject.SetActive(false);
    }

    private void VoteDictionaryChanged(Dictionary<ulong, ulong> previousValue, Dictionary<ulong, ulong> newValue)
    {
        Show();
    }

    private void VotingTimerChanged(float previousValue, float newValue)
    {
        votingTimerSlider.value = newValue / voteManager.VotingTimerMax;
    }

    private void StateChanged(VoteManager.State state)
    {
        switch (state)
        {
            case VoteManager.State.WaitingToVote:
                gameObject.SetActive(false);
                break;
            case VoteManager.State.Voting:
                gameObject.SetActive(true);
                break;
            case VoteManager.State.VoteOver:
                gameObject.SetActive(false);
                break;
        }
    }

    private void Show()
    {
        foreach (Transform child in voteItemParent)
        {
            Destroy(child.gameObject);
        }
        foreach (var vote in voteManager.VoteRegistry.Value)
        {
            VoteItem voteItem = Instantiate(voteItemPrefab, voteItemParent);
            voteItem.Initialise(vote.Key, vote.Value != VoteManager.NO_VOTE);
        }
    }
}