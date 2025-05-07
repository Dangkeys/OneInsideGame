using System;
using IKVM.Reflection.Emit;
using UnityEngine;

public class VoiceChatSystem : MonoBehaviour
{
    private string aliveChannelName = "alive";
    private string deadChannelName = "dead";
    public string CurrentChannelName;
    private void Start()
    {
        var currentLobby = LobbyManager.Instance.CurrentLobby;
        aliveChannelName = currentLobby.Id + "_alive";
        deadChannelName = currentLobby.Id + "_dead";
        CurrentChannelName = aliveChannelName;

        VoiceChatManager.JoinPositionalChannelAsync(CurrentChannelName);
        OneInsideLevelSystem.Instance.PlayerSystem.OnAllPlayersSpawnInTheGame += SubscribeWhenPlayerDied;
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

    private void SubscribeWhenPlayerDied()
    {
        Player player = PlayerSystem.GetLocalPlayerScript();
        player.IsAlive.OnValueChanged += ChangeVoicechatChannel;
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
    }

}
