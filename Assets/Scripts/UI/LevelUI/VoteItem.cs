using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class VoteItem : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Button voteButton;
    [SerializeField] private Image voteImage;
    [SerializeField] private Image selectedImage;
    private ulong playerId;
    private VoteSystem voteManager;

    private void Start() 
    {
        InitializeVoteManager();
    }

    private void InitializeVoteManager()
    {
        if (voteManager == null)
        {
            voteManager = OneInsideLevelSystem.Instance.VoteManager;
            voteManager.VoteRegistry.OnValueChanged += OnVoteDictionaryChanged;
            
            if (voteManager.VoteRegistry.Value.ContainsKey(NetworkManager.Singleton.LocalClientId))
            {
                UpdateVisualState(voteManager.VoteRegistry.Value);
            }
        }
    }

    private void OnDestroy()
    {
        if (voteManager != null)
        {
            voteManager.VoteRegistry.OnValueChanged -= OnVoteDictionaryChanged;
        }
    }

    private void Vote()
    {
        if (voteManager == null)
        {
            InitializeVoteManager();
        }
        voteManager.VoteTargetPlayerServerRpc(playerId);
    }

    private void OnVoteDictionaryChanged(Dictionary<ulong, ulong> previousValue, Dictionary<ulong, ulong> newValue)
    {
        UpdateVisualState(newValue);
    }

    public void Initialise(string playerName,ulong playerId, bool hasVoted)
    {
        this.playerId = playerId;
        if(playerId == NetworkManager.Singleton.LocalClientId)
        {
            voteButton.gameObject.SetActive(false);
        }
        playerNameText.text = playerName;
        voteImage.gameObject.SetActive(hasVoted);

        voteButton.onClick.RemoveAllListeners();
        voteButton.onClick.AddListener(Vote);
        
        InitializeVoteManager();
    }

    private void UpdateVisualState(Dictionary<ulong, ulong> votes)
    {
        if (votes == null) return;

        bool isSelectedByLocalPlayer = false;
        if (votes.TryGetValue(NetworkManager.Singleton.LocalClientId, out ulong votedFor))
        {
            isSelectedByLocalPlayer = votedFor == playerId;
        }

        if (selectedImage != null)
        {
            selectedImage.gameObject.SetActive(isSelectedByLocalPlayer);
        }
    }
}