using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Mono.CSharp;
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
            "Starting Host",
            "Loading Lobby Scene",
            "Lobby Scene Loaded"
        }},
        { LoadingSequence.JoinMatch, new string[]
        {
            "Joining Lobby",
            "Starting Client",
            "Loading Lobby Scene",
            "Lobby Scene Loaded"
        }}
    };


    private CharacterManager characterManager;
    private PerkManager perkManager;
    private AudioManager audioManager;
    private LobbyManager lobbyManager;
    private VoiceChatManager voiceChatManager;


    async void Start()
    {
        await InitializeGame();
    }

    public async Task HostMatch(CreateLobbyDto createLobbyDto)
    {
        UpdateHostMatchProgress(0);
        CreateLobbyAllocationResponseDto responseDto = await lobbyManager.CreateLobbyAsync(createLobbyDto);

        UpdateHostMatchProgress(1);
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(responseDto.Allocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        NetworkManager.Singleton.StartHost();

        UpdateHostMatchProgress(2);
        Loader.LoadNetwork(GameScene.LobbyScene);

        UpdateHostMatchProgress(3);
    }

    public async Task JoinMatch(string identifier, bool useCode)
    {

        UpdateJoinMatchProgress(0);
        JoinLobbyAllocationResponseDto responseDto = await lobbyManager.JoinLobbyAsync(identifier, useCode);

        UpdateJoinMatchProgress(1);
        UnityTransport transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        RelayServerData relayServerData = AllocationUtils.ToRelayServerData(responseDto.JoinAllocation, "dtls");
        transport.SetRelayServerData(relayServerData);

        UpdateJoinMatchProgress(2);
        NetworkManager.Singleton.StartClient();

        UpdateJoinMatchProgress(3);
    }

    public void StartGame()
    {
        Loader.LoadNetwork(GameScene.TajdangScene);
    }

    public async Task InitializeGame()
    {
        characterManager = GetComponentInChildren<CharacterManager>();
        perkManager = GetComponentInChildren<PerkManager>();
        audioManager = GetComponentInChildren<AudioManager>();
        voiceChatManager = GetComponentInChildren<VoiceChatManager>();
        lobbyManager = GetComponentInChildren<LobbyManager>();

        UpdateGameInitializationProgress(0);
        await UnityServices.InitializeAsync();

        UpdateGameInitializationProgress(1);
        AuthState state = await AuthenticationWrapper.DoAuth();

        if (state != AuthState.Authenticated)
        {
            //TODO Add logic to handle failed authentication
            Debug.LogError("Failed to authenticate user");
            return;
        }

        if (AuthenticationService.Instance.PlayerName == null)
        {
            UpdateGameInitializationProgress(2);
            await PlayerNameGenerator.GenerateRandomPlayerName();
        }

        UpdateGameInitializationProgress(3);
        await VivoxService.Instance.InitializeAsync();

        UpdateGameInitializationProgress(4);
        await VivoxService.Instance.LoginAsync();

        UpdateGameInitializationProgress(5);
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

}