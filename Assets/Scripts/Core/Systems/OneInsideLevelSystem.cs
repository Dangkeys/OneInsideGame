using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

using QFSW.QC;

public class OneInsideLevelSystem : SingletonNetwork<OneInsideLevelSystem>
{


    [field: SerializeField] public float GamePlayingTimerMax = 600f;
    [field: SerializeField] public NetworkVariable<float> GamePlayTimer { get; private set; } = new NetworkVariable<float>();
    [field: SerializeField] public NetworkVariable<GameState> State { get; private set; } = new NetworkVariable<GameState>(GameState.WaitingToStart);
    [field: SerializeField] public NetworkVariable<bool> IsEndGameCollapse { get; private set; } = new NetworkVariable<bool>(false);

    [field: SerializeField] public int ImposterAmount { get; private set; } = OneInside.Constants.Player.MAX_IMPOSTERS;
    [field: SerializeField] public PlayerSystem PlayerSystem;
    [field: SerializeField] public VoteSystem VoteSystem;
    [field: SerializeField] public RoleSystem RoleSystem;
    [field: SerializeField] public AbilityAssignment AbilityAssignment;

    [field: SerializeField] public GameObject PlayersContainer;
    [field: SerializeField] public GameObject DeadBodiesContainer;

    public static GameObject Players { get; private set; }
    public static GameObject DeadBodies { get; private set; }

    protected override void Awake()
    {
        base.Awake();
        Players = PlayersContainer;
        DeadBodies = DeadBodiesContainer;
        PlayerSystem.OnCrewMateLost += OnCrewMateLost;
    }

    private void OnCrewMateLost()
    {
        Debug.Log("OnCrewMateLost");
        if (IsServer)
        {
            SetGameState(GameState.ImposterWin);
        }
    }

    public override void OnNetworkSpawn()
    {

        if (IsServer)
        {
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadComplete;
        }
        State.OnValueChanged += StateChanged;


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
            case GameState.CrewmateWin:
                UIManager.Instance.ShowMessage("Crewmate Win");


                break;
            case GameState.ImposterWin:
                UIManager.Instance.ShowMessage("Imposter Win");
                await GameOver();
                break;
        }
    }
    private async Task GameOver()
    {
        if (IsServer)
        {
            if (LobbyManager.Instance.CurrentLobby != null)
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
            State.Value = GameState.ImposterWin;
        }
    }

    [Command]
    public void SetIsEndGameCollapse(bool isEndGameCollapse)
    {
        if (!IsServer)
            return;
        IsEndGameCollapse.Value = isEndGameCollapse;
        Debug.Log("IsEndGameCollapse.Value = " + isEndGameCollapse);
    }

    public void SetGameState(GameState state)
    {
        if (!IsServer)
            return;
        State.Value = state;

    }

    private void OnSceneLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        State.Value = GameState.GamePlaying;
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