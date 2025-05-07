using System;
using IKVM.Reflection.Emit;
using Unity.Services.Vivox;
using UnityEngine;

public class VoiceChatSystem : MonoBehaviour
{
    private string aliveChannelName = "alive";
    private string deadChannelName = "dead";
    public string CurrentChannelName;
    private Player localPlayer;
    private void Start()
    {
        var currentLobby = LobbyManager.Instance.CurrentLobby;

        if (currentLobby != null)
        {
            aliveChannelName = currentLobby.Id + "_alive";
            deadChannelName = currentLobby.Id + "_dead";
        }
        CurrentChannelName = aliveChannelName;


        OneInsideLevelSystem.Instance.PlayerSystem.OnAllPlayersSpawnInTheGame += OnAllPlayersSpawn;
        OneInsideLevelSystem.Instance.VoteSystem.OnStateChanged += VoteStateChanged;
    }

    private void VoteStateChanged(VoteState state)
    {
        switch (state)
        {
            case VoteState.WaitingToVote:
                break;
            case VoteState.Voting:
                VoiceChatManager.LeaveAllChannels();
                VoiceChatManager.JoinGroupChannelAsync(CurrentChannelName);
                break;
            case VoteState.VoteOver:
                VoiceChatManager.LeaveAllChannels();
                VoiceChatManager.JoinPositionalChannelAsync(CurrentChannelName);
                break;



        }
    }

    private async void OnAllPlayersSpawn()
    {
        localPlayer = PlayerSystem.GetLocalPlayerScript();
        await VoiceChatManager.JoinPositionalChannelAsync(CurrentChannelName);
        Set3DPosition();
        Debug.Log("ActiveChannels" + VivoxService.Instance.ActiveChannels.Count);
        localPlayer.PlayerMovement.OnMove += Set3DPosition;
        localPlayer.IsAlive.OnValueChanged += ChangeVoicechatChannel;
    }

    private void Set3DPosition()
    {
        if(VivoxService.Instance.ActiveChannels.ContainsKey(CurrentChannelName))
        {
            VoiceChatManager.Set3DPosition(localPlayer.gameObject, CurrentChannelName);
        }
    }


    private void ChangeVoicechatChannel(bool previousValue, bool newValue)
    {
        if (!newValue && previousValue)
        {
            VoiceChatManager.LeaveAllChannels();
            CurrentChannelName = deadChannelName;
            VoiceChatManager.JoinPositionalChannelAsync(CurrentChannelName);
        }
    }

    private void OnDestroy()
    {
        VoiceChatManager.LeaveAllChannels();
        if (localPlayer != null)
        {
            localPlayer.PlayerMovement.OnMove -= Set3DPosition;
            localPlayer.IsAlive.OnValueChanged -= ChangeVoicechatChannel;
        }
    }

}
