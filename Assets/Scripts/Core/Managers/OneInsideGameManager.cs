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

public class OneInsideGameManager : Singleton<OneInsideGameManager>
{
    public event Action<GameEvent, string> OnGameStateChanged;
    


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
        ConnectionManager.Instance.LeaveMatch();
        await LobbyManager.Instance.LeaveLobbyAsync();
    }

    public async Task KickPlayerAsync(ulong clientId)
    {
        ConnectionManager.Instance.KickPlayer(clientId);
        await LobbyManager.Instance.KickPlayerAsync(clientId.ToString());
    }

    public async Task HostMatch(CreateLobbyDto createLobbyDto)
    {

        NotifyGameStateChanged(GameEvent.CreatingLobby);
        
        CreateLobbyAllocationResponseDto responseDto = await LobbyManager.Instance.CreateLobbyAsync(createLobbyDto);
        if (responseDto == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to create lobby");
            return;
        }

        ConnectionManager.InitializeHostRelayTransport(responseDto);


        NotifyGameStateChanged(GameEvent.LoadingLobbyScene);
        
        await Loader.LoadNetwork(GameScene.LobbyScene);

        NotifyGameStateChanged(GameEvent.LobbySceneLoaded);
    }

    public async Task QuickJoinMatchAsync()
    {
        NotifyGameStateChanged(GameEvent.JoiningLobby);
        
        JoinLobbyAllocationResponseDto responseDto = await LobbyManager.Instance.QuickJoinAsync();

        if (responseDto == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to join lobby");
            return;
        }

        ConnectionManager.InitializeClientRelayTransport(responseDto);

        NotifyGameStateChanged(GameEvent.LoadingLobbyScene);
        
        await Loader.LoadNetwork(GameScene.LobbyScene);

        NotifyGameStateChanged(GameEvent.LobbySceneLoaded);
    }

    public async Task JoinMatchByCodeAsync(string joinCode)
    {
        NotifyGameStateChanged(GameEvent.JoiningLobby);
        
        JoinLobbyAllocationResponseDto responseDto = await LobbyManager.Instance.JoinLobbyByCodeAsync(joinCode);

        if (responseDto == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to join lobby");
            return;
        }

        ConnectionManager.InitializeClientRelayTransport(responseDto);

        NotifyGameStateChanged(GameEvent.LoadingLobbyScene);
        
        await Loader.LoadNetwork(GameScene.LobbyScene);

        NotifyGameStateChanged(GameEvent.LobbySceneLoaded);
    }

    public async Task JoinMatchByLobbyIdAsync(string lobbyId)
    {
        NotifyGameStateChanged(GameEvent.JoiningLobby);
        
        JoinLobbyAllocationResponseDto responseDto = await LobbyManager.Instance.JoinLobbyByIdAsync(lobbyId);
        if (responseDto == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to join lobby");
            return;
        }
        
        ConnectionManager.InitializeClientRelayTransport(responseDto);

        NotifyGameStateChanged(GameEvent.LoadingLobbyScene);
        
        await Loader.LoadNetwork(GameScene.LobbyScene);

        NotifyGameStateChanged(GameEvent.LobbySceneLoaded);
    }

    public async Task StartGame()
    {
        var currentLobby = LobbyManager.Instance.CurrentLobby;
        if (currentLobby == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to start game, no lobby found");
            return;
        }

        await LobbyManager.Instance.UpdateCurrentLobbyAsync(new UpdateLobbyDto(isLocked: true));
        
        NotifyGameStateChanged(GameEvent.StartingGame);
        
        await Loader.LoadNetwork(GameScene.MainScene);
        
        NotifyGameStateChanged(GameEvent.GameStarted);
    }
    
    [Command]
    public async Task StopGame()
    {
        var currentLobby = LobbyManager.Instance.CurrentLobby;
        if (currentLobby == null)
        {
            NotifyGameStateChanged(GameEvent.OperationFailed, "Failed to stop game, no lobby found");
            return;
        }

        await LobbyManager.Instance.UpdateCurrentLobbyAsync(new UpdateLobbyDto(isLocked: false));
        
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
        if (string.IsNullOrEmpty(message))
        {
            message = gameEvent.ToString();
        }
        
        OnGameStateChanged?.Invoke(gameEvent, message);
    }
}