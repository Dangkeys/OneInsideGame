using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.CSharp;
using QFSW.QC;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Vivox;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OneInsideGameManager : SingletonPersistent<OneInsideGameManager>
{
    public event Action<float, string> OnLoadingProgressChanged;
    public event Action<string, Action, Action> OnConfirmationRequired;

    public event Action<string> OnShowMessageRequired;
    public enum LoadingSequence
    {
        GameInitialization,
        HostMatch,
        JoinMatch
    }
    private readonly Dictionary<LoadingSequence, string[]> loadingDictionary = new Dictionary<LoadingSequence, string[]>()
    {
        { LoadingSequence.GameInitialization, new string[]
        {
            "Initializing Unity Services",
            "Authenticating User",
            "Generated Player Name",
            "Initializing Vivox Service",
            "Logging into Vivox Service",
            "Loading Main Menu",
            "Game Initialized"
        }},
        { LoadingSequence.HostMatch, new string[]
        {
            "Creating Lobby",
            "Loading Lobby Scene",
            "Lobby Scene Loaded"
        }},
        { LoadingSequence.JoinMatch, new string[]
        {
            "Joining Lobby",
            "Loading Lobby Scene",
            "Lobby Scene Loaded"
        }}
    };


    public CharacterManager CharacterManager { get; private set; }
    public PerkManager PerkManager { get; private set; }
    public AudioManager AudioManager { get; private set; }
    public LobbyManager LobbyManager { get; private set; }
    public VoiceChatManager VoiceChatManager { get; private set; }
    public NetcodeManager NetcodeManager { get; private set; }
    protected override void OnAwakeInitialization()
    {
        base.OnAwakeInitialization();
        CharacterManager = GetComponentInChildren<CharacterManager>(true); // true to include inactive objects
        PerkManager = GetComponentInChildren<PerkManager>(true);
        AudioManager = GetComponentInChildren<AudioManager>(true);
        VoiceChatManager = GetComponentInChildren<VoiceChatManager>(true);
        LobbyManager = GetComponentInChildren<LobbyManager>(true);
        NetcodeManager = GetComponentInChildren<NetcodeManager>(true);

        if (CharacterManager == null)
            Debug.LogError("CharacterManager not found!");
        if (PerkManager == null)
            Debug.LogError("PerkManager not found!");
        if (AudioManager == null)
            Debug.LogError("AudioManager not found!");
        if (VoiceChatManager == null)
            Debug.LogError("VoiceChatManager not found!");
        if (LobbyManager == null)
            Debug.LogError("LobbyManager not found!");
        if (NetcodeManager == null)
            Debug.LogError("NetcodeManager not found!");
    }

    async void Start()
    {
        if (SceneManager.GetActiveScene().name == GameScene.BootstrapScene.ToString())
        {
            await InitializeGameAsync();
        }
    }

    [Command]
    public async Task LeaveMatchAsync()
    {
        //TODO migrate host(I don't think we can migrate in the current version of the netcode)

        NetcodeManager.LeaveMatch();
        await LobbyManager.LeaveLobbyAsync();
    }

    public async Task KickPlayerAsync(ulong clientId)
    {
        NetcodeManager.KickPlayer(clientId);
        await LobbyManager.KickPlayerAsync(clientId.ToString());
    }

    public async Task HostMatch(CreateLobbyDto createLobbyDto)
    {
        UpdateHostMatchProgress(1);
        CreateLobbyAllocationResponseDto responseDto = await LobbyManager.CreateLobbyAsync(createLobbyDto);
        if (responseDto == null)
        {
            OnLoadingProgressChanged?.Invoke(1, "Failed to create lobby");
            return;
        }


        NetcodeManager.InitializeHostRelayTransport(responseDto);

        await Loader.LoadNetwork(GameScene.LobbyScene);

        UpdateHostMatchProgress(2);
    }

    public async Task QuickJoinMatchAsync()
    {
        UpdateJoinMatchProgress(1);
        JoinLobbyAllocationResponseDto responseDto = await LobbyManager.QuickJoinAsync();

        if (responseDto == null)
        {
            OnLoadingProgressChanged?.Invoke(1, "Failed to join lobby");
            return;
        }

        NetcodeManager.InitializeClientRelayTransport(responseDto);

    }



    public async Task JoinMatchByCodeAsync(string joinCode)
    {
        UpdateJoinMatchProgress(1);
        JoinLobbyAllocationResponseDto responseDto = await LobbyManager.JoinLobbyByCodeAsync(joinCode);

        if (responseDto == null)
        {
            OnLoadingProgressChanged?.Invoke(1, "Failed to join lobby");
            return;
        }

        NetcodeManager.InitializeClientRelayTransport(responseDto);

    }

    public async Task JoinMatchByLobbyIdAsync(string lobbyId)
    {
        UpdateJoinMatchProgress(1);
        JoinLobbyAllocationResponseDto responseDto = await LobbyManager.JoinLobbyByIdAsync(lobbyId);
        if (responseDto == null)
        {
            OnLoadingProgressChanged?.Invoke(1, "Failed to join lobby");
            return;
        }
        NetcodeManager.InitializeClientRelayTransport(responseDto);

    }


    public async Task StartGame()
    {
        var currentLobby = LobbyManager.CurrentLobby;
        if (currentLobby == null)
        {
            ShowMessage("Failed to start game, no lobby found");
            return;
        }

        await LobbyManager.UpdateCurrentLobbyAsync(new UpdateLobbyDto(isLocked: true));
        OnLoadingProgressChanged?.Invoke(0.5f, "Starting Game");
        await Loader.LoadNetwork(GameScene.TajdangScene);
        OnLoadingProgressChanged?.Invoke(1, "Game Started");
    }
    public async Task StopGame()
    {
        var currentLobby = LobbyManager.CurrentLobby;
        if (currentLobby == null)
        {
            ShowMessage("Failed to stop game, no lobby found");
            return;
        }

        await LobbyManager.UpdateCurrentLobbyAsync(new UpdateLobbyDto(isLocked: false));
        OnLoadingProgressChanged?.Invoke(0.5f, "Stopping Game");
        await Loader.LoadNetwork(GameScene.LobbyScene);
        OnLoadingProgressChanged?.Invoke(1, "Game Stopped");
    }

    public async Task InitializeGameAsync(bool shouldLoadScene = true)
    {

        UpdateGameInitializationProgress(0);
        await UnityServices.InitializeAsync();

        UpdateGameInitializationProgress(1);
        AuthState state = await AuthenticationWrapper.DoAuth();

        if (state != AuthState.Authenticated)
        {
            //TODO Add more logic to handle failed authentication
            Debug.LogError("Failed to authenticate user");
            return;
        }

        if (AuthenticationService.Instance.PlayerName == null)
        {
            UpdateGameInitializationProgress(2);
            await PlayerNameGenerator.GenerateRandomPlayerName();
        }

        //TODO Fix vivox service to handle failed login or InitializeAsync
        try
        {
            UpdateGameInitializationProgress(3);
            await VivoxService.Instance.InitializeAsync();

            UpdateGameInitializationProgress(4);
            await VivoxService.Instance.LoginAsync();

        }
        catch (RequestFailedException e)
        {
            UpdateGameInitializationProgress(6);
            Debug.LogWarning(e);
        }

        UpdateGameInitializationProgress(5);
        if (shouldLoadScene)
            await SceneManager.LoadSceneAsync(GameScene.MainMenuScene.ToString());
        UpdateGameInitializationProgress(6);
    }


    private void UpdateProgress(LoadingSequence loadingSequence, int currentStep)
    {
        string[] steps = loadingDictionary[loadingSequence];
        float progress = Mathf.Clamp((float)currentStep / (steps.Length - 1), 0, 1);
        OnLoadingProgressChanged?.Invoke(progress, steps[currentStep]);
    }
    private void UpdateGameInitializationProgress(int currentStep)
    {
        UpdateProgress(LoadingSequence.GameInitialization, currentStep);
    }

    private void UpdateHostMatchProgress(int currentStep)
    {
        UpdateProgress(LoadingSequence.HostMatch, currentStep);
    }

    private void UpdateJoinMatchProgress(int currentStep)
    {
        UpdateProgress(LoadingSequence.JoinMatch, currentStep);
    }
    public void ShowConfirmation(string message, Action onConfirm, Action onCancel = null)
    {
        OnConfirmationRequired?.Invoke(message, onConfirm, onCancel);
    }

    public void ShowMessage(string message)
    {
        OnShowMessageRequired?.Invoke(message);
    }

    public void ShowProgressChanged(float progress, string message)
    {
        OnLoadingProgressChanged?.Invoke(progress, message);
    }


}