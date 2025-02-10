
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
        if(muteInputToggle == null || muteOutputToggle == null)
        {
            Debug.Log("Mute input or output toggle is not assigned");
            return;
        }
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
        
    }
    private async void OnDestroy()
    {
        await VivoxService.Instance.LeaveAllChannelsAsync();
        await VivoxService.Instance.LogoutAsync();
    }
}