using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.CSharp;
using QFSW.QC;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using Unity.Services.Vivox;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OneInsideGameManager : SingletonPersistent<OneInsideGameManager>
{
    // Simple events for game state changes
    public event Action<GameEvent, string> OnGameStateChanged;

    // Game events that can trigger UI updates
    public enum GameEvent
    {
        // Game initialization events
        InitializingServices,
        AuthenticatingUser,
        GeneratingPlayerName,
        InitializingVivox,
        LoggingIntoVivox,
        LoadingMainMenu,
        GameInitialized,
        
        // Host match events
        CreatingLobby,
        LoadingLobbyScene,
        LobbySceneLoaded,
        
        // Join match events
        JoiningLobby,
        
        // General events
        OperationFailed,
        StartingGame,
        GameStarted,
        StoppingGame,
        GameStopped
    }

    public CharacterManager CharacterManager { get; private set; }
    public PerkManager PerkManager { get; private set; }
    public AudioManager AudioManager { get; private set; }
    public LobbyManager LobbyManager { get; private set; }
    public VoiceChatManager VoiceChatManager { get; private set; }
    public NetcodeManager NetcodeManager { get; private set; }
    public UIManager UIManager { get; set; }
    
    protected override void OnAwakeInitialization()
    {
        base.OnAwakeInitialization();
        CharacterManager = GetComponentInChildren<CharacterManager>(true);
        PerkManager = GetComponentInChildren<PerkManager>(true);
        AudioManager = GetComponentInChildren<AudioManager>(true);
        VoiceChatManager = GetComponentInChildren<VoiceChatManager>(true);
        LobbyManager = GetComponentInChildren<LobbyManager>(true);
        NetcodeManager = GetComponentInChildren<NetcodeManager>(true);
        UIManager = GetComponentInChildren<UIManager>(true);

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
        if (UIManager == null)
            Debug.LogError("UIManager not found!");
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
        // Notify about creating lobby
        NotifyGameStateChanged(GameEvent.CreatingLobby);
        
        CreateLobbyAllocationResponseDto responseDto = await LobbyManager.CreateLobbyAsync(createLobbyDto);
        if (responseDto == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to create lobby");
            return;
        }

        NetcodeManager.InitializeHostRelayTransport(responseDto);

        // Notify about loading lobby scene
        NotifyGameStateChanged(GameEvent.LoadingLobbyScene);
        
        await Loader.LoadNetwork(GameScene.LobbyScene);
        
        // Notify about lobby scene loaded
        NotifyGameStateChanged(GameEvent.LobbySceneLoaded);
    }

    public async Task QuickJoinMatchAsync()
    {
        NotifyGameStateChanged(GameEvent.JoiningLobby);
        
        JoinLobbyAllocationResponseDto responseDto = await LobbyManager.QuickJoinAsync();

        if (responseDto == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to join lobby");
            return;
        }

        NetcodeManager.InitializeClientRelayTransport(responseDto);
        
        // Notify about loading lobby scene
        NotifyGameStateChanged(GameEvent.LoadingLobbyScene);
        
        await Loader.LoadNetwork(GameScene.LobbyScene);
        
        // Notify about lobby scene loaded
        NotifyGameStateChanged(GameEvent.LobbySceneLoaded);
    }

    public async Task JoinMatchByCodeAsync(string joinCode)
    {
        NotifyGameStateChanged(GameEvent.JoiningLobby);
        
        JoinLobbyAllocationResponseDto responseDto = await LobbyManager.JoinLobbyByCodeAsync(joinCode);

        if (responseDto == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to join lobby");
            return;
        }

        NetcodeManager.InitializeClientRelayTransport(responseDto);
        
        // Notify about loading lobby scene
        NotifyGameStateChanged(GameEvent.LoadingLobbyScene);
        
        await Loader.LoadNetwork(GameScene.LobbyScene);
        
        // Notify about lobby scene loaded
        NotifyGameStateChanged(GameEvent.LobbySceneLoaded);
    }

    public async Task JoinMatchByLobbyIdAsync(string lobbyId)
    {
        NotifyGameStateChanged(GameEvent.JoiningLobby);
        
        JoinLobbyAllocationResponseDto responseDto = await LobbyManager.JoinLobbyByIdAsync(lobbyId);
        if (responseDto == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to join lobby");
            return;
        }
        
        NetcodeManager.InitializeClientRelayTransport(responseDto);
        
        // Notify about loading lobby scene
        NotifyGameStateChanged(GameEvent.LoadingLobbyScene);
        
        await Loader.LoadNetwork(GameScene.LobbyScene);
        
        // Notify about lobby scene loaded
        NotifyGameStateChanged(GameEvent.LobbySceneLoaded);
    }

    public async Task StartGame()
    {
        var currentLobby = LobbyManager.CurrentLobby;
        if (currentLobby == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to start game, no lobby found");
            return;
        }

        await LobbyManager.UpdateCurrentLobbyAsync(new UpdateLobbyDto(isLocked: true));
        
        NotifyGameStateChanged(GameEvent.StartingGame);
        
        await Loader.LoadNetwork(GameScene.MainScene);
        
        NotifyGameStateChanged(GameEvent.GameStarted);
    }
    
    [Command]
    public async Task StopGame()
    {
        var currentLobby = LobbyManager.CurrentLobby;
        if (currentLobby == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to stop game, no lobby found");
            return;
        }

        await LobbyManager.UpdateCurrentLobbyAsync(new UpdateLobbyDto(isLocked: false));
        
        NotifyGameStateChanged(GameEvent.StoppingGame);
        
        await Loader.LoadNetwork(GameScene.LobbyScene);
        
        NotifyGameStateChanged(GameEvent.GameStopped);
    }

    public async Task InitializeGameAsync(bool shouldLoadScene = true)
    {
        NotifyGameStateChanged(GameEvent.InitializingServices);
        await UnityServices.InitializeAsync();

        NotifyGameStateChanged(GameEvent.AuthenticatingUser);
        AuthState state = await AuthenticationWrapper.DoAuth();

        if (state != AuthState.Authenticated)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to authenticate user");
            return;
        }

        if (AuthenticationService.Instance.PlayerName == null)
        {
            NotifyGameStateChanged(GameEvent.GeneratingPlayerName);
            await PlayerNameGenerator.GenerateRandomPlayerName();
        }

        try
        {
            NotifyGameStateChanged(GameEvent.InitializingVivox);
            await VivoxService.Instance.InitializeAsync();

            NotifyGameStateChanged(GameEvent.LoggingIntoVivox);
            await VivoxService.Instance.LoginAsync();
        }
        catch (RequestFailedException e)
        {
            Debug.LogWarning(e);
            // Continue despite Vivox errors
        }

        if (shouldLoadScene)
        {
            NotifyGameStateChanged(GameEvent.LoadingMainMenu);
            await SceneManager.LoadSceneAsync(GameScene.MainMenuScene.ToString());
        }
        
        NotifyGameStateChanged(GameEvent.GameInitialized);
    }

    private void NotifyGameStateChanged(GameEvent gameEvent, string message = null)
    {
        // If no custom message provided, use the event name as the message
        if (string.IsNullOrEmpty(message))
        {
            message = gameEvent.ToString();
        }
        
        OnGameStateChanged?.Invoke(gameEvent, message);
    }
}