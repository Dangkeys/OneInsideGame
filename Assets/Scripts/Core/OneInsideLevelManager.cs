using System;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OneInsideLevelManager : NetworkBehaviour
{


    [field: SerializeField] public float GamePlayingTimerMax = 600f;
    public NetworkVariable<float> GamePlayTimer = new NetworkVariable<float>();
    public NetworkVariable<GameState> State = new NetworkVariable<GameState>(GameState.WaitingToStart);

    [field: SerializeField] public PlayerManager PlayerManager;
    [field: SerializeField] public VoteManager VoteManager;

    [field: SerializeField] public RoleManager RoleManager;

    public NetworkVariable<bool> IsPlayerInitializationRequired = new NetworkVariable<bool>(true);

    public static OneInsideLevelManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public async override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadComplete;
            State.OnValueChanged += OnGameStateChanged;
        }

        if(UnityServices.State == ServicesInitializationState.Uninitialized)
        {
            await ClientSingleton.Instance.CreateClient();
        }

    }

    private void OnGameStateChanged(GameState previousValue, GameState newValue)
    {
        switch (newValue)
        {
            case GameState.WaitingToStart:
                break;
            case GameState.GamePlaying:
                GamePlayTimer.Value = GamePlayingTimerMax;
                break;
            case GameState.GameOver:
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

    public override void OnNetworkDespawn()
    {
        if (!IsServer)
            return;
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadComplete;
    }



    private void OnSceneLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        if (sceneName != GameScene.TajdangScene.ToString())
            return;
        IsPlayerInitializationRequired.Value = false;
        State.Value = GameState.GamePlaying;
    }
}