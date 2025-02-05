
using System;
using System.Linq;
using System.Threading.Tasks;
using QFSW.QC;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.UI;

public class VivoxManager : MonoBehaviour
{
    [SerializeField] private Button loginButton;
    [SerializeField] private Button logoutButton;
    [SerializeField] private Button joinChannelButton;
    [SerializeField] private Button joinTestChannelButton;
    [SerializeField] private Button leaveChannelButton;

    [SerializeField] private Toggle muteInputToggle;
    [SerializeField] private Toggle muteOutputToggle;

    [SerializeField] private GameObject currentUsers;

    [SerializeField] private GameObject user;

    private const string CHANNEL_NAME = "Room1";

    private async void Start()
    {
        Debug.Log("Vivox Starting...");
        if (UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await ClientSingleton.Instance.CreateClient();
        }
        Debug.Log("Vivox Initialized");
        loginButton.onClick.AddListener(async () => await Login());
        logoutButton.onClick.AddListener(async () => await Logout());
        joinChannelButton.onClick.AddListener(async () => await JoinInChannel(CHANNEL_NAME));
        joinTestChannelButton.onClick.AddListener(async () => await JoinInTestChannel());
        leaveChannelButton.onClick.AddListener(async () => await LeaveChannel());
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
        VivoxService.Instance.ParticipantAddedToChannel += OnParticipantAddedToChannel;
        VivoxService.Instance.ChannelMessageReceived += OnChannelMessageReceived;

    }
    private async void OnDestroy()
    {
        await VivoxService.Instance.LeaveAllChannelsAsync();
        await VivoxService.Instance.LogoutAsync();
        VivoxService.Instance.ParticipantAddedToChannel -= OnParticipantAddedToChannel;
        VivoxService.Instance.ChannelMessageReceived -= OnChannelMessageReceived;
    }
    private void OnChannelMessageReceived(VivoxMessage message)
    {
        string messageText = message.MessageText;
        string senderID = message.SenderPlayerId;
        string senderDisplayName = message.SenderDisplayName;
        string messageChannel = message.ChannelName;
        Debug.Log($"Message Received: {messageText} from {senderID} in {messageChannel}");
    }

    private void OnParticipantAddedToChannel(VivoxParticipant participant)
    {
        var userInstance = Instantiate(user, currentUsers.transform);
        userInstance.GetComponentInChildren<Text>().text = participant.PlayerId;
        userInstance.SetActive(true);
        bool isCurrentlySpeaking = false;

        participant.ParticipantAudioEnergyChanged += () =>
        {
            if (participant.AudioEnergy > 0.01 && !isCurrentlySpeaking)
            {
                Debug.Log($"{participant.DisplayName} started talking");
                isCurrentlySpeaking = true;
            }
            else if (participant.AudioEnergy < 0.01 && isCurrentlySpeaking)
            {
                Debug.Log($"{participant.DisplayName} stopped talking");
                isCurrentlySpeaking = false;
            }
        };
        if (userInstance.TryGetComponent<Toggle>(out var toggle))
        {
            toggle.isOn = false;
            toggle.onValueChanged.AddListener((bool isToggle) =>
            {
                Debug.Log("Mute: " + isToggle);
                Debug.Log("Mute: " + participant.PlayerId);
                if (isToggle)
                {
                    participant.MutePlayerLocally();
                }
                else
                {
                    participant.UnmutePlayerLocally();
                }

            });
        }

    }

    public async static Task Login()
    {
        Debug.Log("Vivox Logging In...");
        var loginOptions = new LoginOptions
        {
            DisplayName = AuthenticationService.Instance.PlayerId,
        };
        if(VivoxService.Instance.IsLoggedIn)
        {
            await VivoxService.Instance.LogoutAsync();
        }

        await VivoxService.Instance.LoginAsync(loginOptions);
        Debug.Log("Vivox Logged In");
    }

    public async Task JoinInChannel(string channelName)
    {
        Debug.Log("Joining Channel...");
        var channel3DProperties = new Channel3DProperties(100,50,60,AudioFadeModel.ExponentialByDistance);
        await VivoxService.Instance.JoinPositionalChannelAsync(channelName, ChatCapability.AudioOnly, channel3DProperties);
        VivoxService.Instance.Set3DPosition(currentUsers, CHANNEL_NAME);
        Debug.Log("Joined Channel");
    }

    public async static Task Logout()
    {
        Debug.Log("Logging Out...");
        await VivoxService.Instance.LogoutAsync();
        Debug.Log("Logged Out");
    }


    public async static Task JoinInTestChannel()
    {
        Debug.Log("Joining Channel...");
        await VivoxService.Instance.JoinEchoChannelAsync(AuthenticationService.Instance.PlayerId, ChatCapability.TextAndAudio);
        Debug.Log("Joined Channel");
    }
    public async static Task LeaveChannel()
    {
        Debug.Log("Leaving Channel...");
        await VivoxService.Instance.LeaveAllChannelsAsync();
        Debug.Log("Left Channel");
    }



    [Command]
    public async static Task SendMessageToChannel()
    {
        Debug.Log("Sending Message...");
        await VivoxService.Instance.SendChannelTextMessageAsync(CHANNEL_NAME, "Hello World");
        Debug.Log("Message Sent");
    }

    [Command]
    void ListInputDevices()
    {
        var devices = VivoxService.Instance.AvailableInputDevices;
        for (int i = 0; i < devices.Count; i++)
        {
            Debug.Log($"Index: {i}, Device Name: {devices[i].DeviceName}, Device ID: {devices[i].DeviceID}");
        }
    }

    [Command]
    void ListOutputDevices()
    {
        var devices = VivoxService.Instance.AvailableOutputDevices;
        for (int i = 0; i < devices.Count; i++)
        {
            Debug.Log($"Index: {i}, Device Name: {devices[i].DeviceName}, Device ID: {devices[i].DeviceID}");
        }
    }

    [Command]
    async void SetInputDevice(int index)
    {
        var devices = VivoxService.Instance.AvailableInputDevices;
        if (index >= 0 && index < devices.Count)
        {
            await VivoxService.Instance.SetActiveInputDeviceAsync(devices[index]);
            Debug.Log($"Set input device to: {devices[index].DeviceName}");
        }
        else
        {
            Debug.LogError($"Invalid input device index: {index}. Please use ListInputDevices to see available indices.");
        }
    }

    [Command]
    async void SetOutputDevice(int index)
    {
        var devices = VivoxService.Instance.AvailableOutputDevices;
        if (index >= 0 && index < devices.Count)
        {
            await VivoxService.Instance.SetActiveOutputDeviceAsync(devices[index]);
            Debug.Log($"Set output device to: {devices[index].DeviceName}");
        }
        else
        {
            Debug.LogError($"Invalid output device index: {index}. Please use ListOutputDevices to see available indices.");
        }
    }

    void Mute()
    {
        
    }

}