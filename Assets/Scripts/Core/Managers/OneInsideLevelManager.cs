using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OneInsideLevelManager : NetworkBehaviour
{


    [field: SerializeField] public float GamePlayingTimerMax = 600f;
    public NetworkVariable<float> GamePlayTimer = new NetworkVariable<float>();
    public NetworkVariable<GameState> State = new NetworkVariable<GameState>(GameState.WaitingToStart);

    [field: SerializeField] public int ImposterAmount { get; private set; } = OneInside.Constants.Player.MAX_IMPOSTERS;
    [field: SerializeField] public PlayerManager PlayerManager;
    [field: SerializeField] public VoteManager VoteManager;
    [field: SerializeField] public RoleManager RoleManager;
    // [field: SerializeField] public VivoxManager VivoxManager;

    [field: SerializeField] public GameObject PlayersContainer;
    [field: SerializeField] public GameObject DeadBodiesContainer;


    public static OneInsideLevelManager Instance { get; private set; }
    private OneInsideGameManager oneInsideGameManager;
    public static GameObject Players { get; private set; }
    public static GameObject DeadBodies { get; private set; }

    private void Awake()
    {
        Instance = this;
        oneInsideGameManager = OneInsideGameManager.Instance;
        Players = PlayersContainer;
        DeadBodies = DeadBodiesContainer;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadComplete;
            State.OnValueChanged += StateChanged;
        }

    }

    private async void StateChanged(GameState previousValue, GameState newValue)
    {
        await OnGameStateChanged(previousValue, newValue);
    }

    private async Task OnGameStateChanged(GameState previousValue, GameState newValue)
    {
        switch (newValue)
        {
            case GameState.WaitingToStart:
                break;
            case GameState.GamePlaying:
                if (IsServer)
                {
                    GamePlayTimer.Value = GamePlayingTimerMax;
                }
                break;
            case GameState.GameOver:
                if (IsServer)
                {
                    if (OneInsideGameManager.Instance.LobbyManager.CurrentLobby != null)
                    {
                        await OneInsideGameManager.Instance.LeaveMatchAsync();

                        //TODO fix the stop game function
                        // PlayerManager.ClearAllPlayers();
                        // await OneInsideGameManager.Instance.StopGame();
                    }
                    else
                    {
                        NetworkManager.Singleton.Shutdown();
                    }
                }

                break;
        }
    }

    private void Update()
    {
        if (!IsServer)
            return;
        if (State.Value == GameState.GamePlaying)
        {
            UpdateGamePlayingTimer();
        }
    }

    private void UpdateGamePlayingTimer()
    {
        if (GamePlayTimer.Value <= 0)
            return;

        GamePlayTimer.Value -= Time.deltaTime;

        if (GamePlayTimer.Value < 0)
        {
            State.Value = GameState.GameOver;
        }
    }

    public void SetGameState(GameState state)
    {
        if (!IsServer)
            return;
        State.Value = state;

    }

    private void OnSceneLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName == GameScene.LobbyScene.ToString())
        {
            State.Value = GameState.GamePlaying;
        }
    }
    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            if (State != null)
            {
                State.Value = GameState.WaitingToStart;
            }
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadComplete;
            State.OnValueChanged -= StateChanged;
        }

    }
}