using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerVoiceChat : NetworkBehaviour
{
    private Player player;
    private string aliveAudioChannel = "aliveAudioChannel";

    private string deadAudioChannel = "deadAudioChannel";

    private string currentAudioChannel;
    private readonly Channel3DProperties audio3DConfiguration = new Channel3DProperties(15, 7, 1, AudioFadeModel.InverseByDistance);
    private void Awake()
    {
        currentAudioChannel = aliveAudioChannel;
        player = GetComponent<Player>();
        // if (OneInsideLevelManager.Instance.IsLoadedFromLobbyScene.Value)
        // {
                //add lobbyId to the channel name
        // }
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAddedToChannel;
    }

    private void OnParticipantAddedToChannel(VivoxParticipant participant)
    {
        if (!IsOwner)
            return;
        Debug.Log($"Participant {participant.DisplayName} added to channel {currentAudioChannel}");
    }

    public override async void OnNetworkSpawn()
    {

        if (OneInsideLevelManager.Instance == null)
            return;
        if (!IsOwner)
            return;

        try
        {
            const float LOGIN_TIMEOUT_SECONDS = 30f;
            float elapsed = 0f;

            while (!VivoxService.Instance.IsLoggedIn)
            {
                await Task.Delay(100);
                elapsed += 0.1f;

                if (elapsed >= LOGIN_TIMEOUT_SECONDS)
                {
                    Debug.LogError("Vivox login timeout exceeded");
                    return;
                }
            }




            try
            {
                await VivoxService.Instance.JoinPositionalChannelAsync(
                    currentAudioChannel,
                    ChatCapability.AudioOnly,
                    audio3DConfiguration
                );
                player.IsAlive.OnValueChanged += OnPlayerAliveChanged;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to join Vivox channel: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error in OnNetworkSpawn Vivox setup: {ex.Message}");
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;
        UpdateAudioPositionChannel();
    }
    private async void OnPlayerAliveChanged(bool previousValue, bool newValue)
    {
        if (newValue)
            return;

        await VivoxService.Instance.LeaveAllChannelsAsync();
        currentAudioChannel = deadAudioChannel;
        await VivoxService.Instance.JoinPositionalChannelAsync(deadAudioChannel, ChatCapability.AudioOnly, audio3DConfiguration);
        UpdateAudioPositionChannel();
    }

    private void UpdateAudioPositionChannel()
    {
        if (!IsOwner)
            return;
        if (!VivoxService.Instance.ActiveChannels.ContainsKey(currentAudioChannel))
            return;
        VivoxService.Instance.Set3DPosition(gameObject, currentAudioChannel);
    }
}
