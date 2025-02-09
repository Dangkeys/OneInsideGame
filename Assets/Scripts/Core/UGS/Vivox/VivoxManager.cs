
using System;
using System.Linq;
using System.Threading.Tasks;
using QFSW.QC;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class VivoxManager : Singleton<VivoxManager>
{
    [SerializeField] private Toggle muteInputToggle;
    [SerializeField] private Toggle muteOutputToggle;

    private async void Start()
    {
        Debug.Log("Vivox Starting...");
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await ClientSingleton.Instance.CreateClient();
        }
        Debug.Log("Vivox Initialized");
        muteInputToggle.onValueChanged.AddListener((bool isToggle) =>
        {
            if (isToggle)
            {
                VivoxService.Instance.MuteInputDevice();
            }
            else
            {
                VivoxService.Instance.UnmuteInputDevice();
            }
        });
        muteOutputToggle.onValueChanged.AddListener((bool isToggle) =>
        {
            if (isToggle)
            {
                VivoxService.Instance.MuteOutputDevice();
            }
            else
            {
                VivoxService.Instance.UnmuteOutputDevice();
            }
        });
        VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;
        
    }
    private async void OnDestroy()
    {
        await VivoxService.Instance.LeaveAllChannelsAsync();
        await VivoxService.Instance.LogoutAsync();
        VivoxService.Instance.ChannelMessageReceived -= OnChannelMessageReceived;
    }
    private void OnChannelMessageReceived(VivoxMessage message)
    {
        string messageText = message.MessageText;
        string senderID = message.SenderPlayerId;
        string senderDisplayName = message.SenderDisplayName;
        string messageChannel = message.ChannelName;
    }
}